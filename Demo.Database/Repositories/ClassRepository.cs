using Demo.Application.Repositories;
using Demo.Core.Enums;
using Demo.Core.Models;
using Demo.Core.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using Demo.Application.ViewModels;

namespace Demo.Database.Repositories
{
    public class ClassRepository : BaseRepository<Class>, IClassRepository
    {
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;

        public ClassRepository(
            IMongoDatabase db,
            IUserRepository userRepository,
            IOrderRepository orderRepository
        ) : base(db)
        {
            _userRepository = userRepository;
            _orderRepository = orderRepository;
        }

        public async Task<Class?> GetByIdAsync(Guid id)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Class>> GetClassesByCourseIdAsync(Guid courseId)
        {
            var classes = await GetAllAsync();
            return classes
                .Where(c => c.Course != null && c.Course.Id == courseId && c.StudentIds.Count < 25)
                .ToList();
        }

        public async Task<List<ApprovedStudentViewModel>> GetApprovedStudentsWithCourseAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            var approvedOrders = orders.Where(o => o.Status == OrderStatus.Approved).ToList();

            var usernames = approvedOrders.Select(o => o.Username).Distinct().ToList();
            var users = await _userRepository.GetAllAsync();

            var approvedUsers = users.Where(u => usernames.Contains(u.UserName)).ToList();

            var userDict = approvedUsers.ToDictionary(u => u.UserName, u => u);
            var userCourses = approvedOrders
                .Where(o => userDict.ContainsKey(o.Username))
                .Select(o => new ApprovedStudentViewModel
                {
                    Id = ToGuid(userDict[o.Username].Id),
                    FullName = userDict[o.Username].FullName,
                    Email = userDict[o.Username].Email,
                    RegisteredCourseId = o.Course.Id,
                    CourseTitle = o.Course.Title
                })
                .DistinctBy(x => x.Id) // nếu nhiều đơn hàng → tránh duplicate
                .ToList();

            // Loại bỏ học viên đã được gán vào lớp
            var allClasses = await GetAllAsync();
            var assignedStudentIds = allClasses.SelectMany(c => c.StudentIds).Distinct().ToHashSet();

            return userCourses
                .Where(x => !assignedStudentIds.Contains(x.Id))
                .ToList();
        }

        public async Task<List<User>> GetStudentsInClassAsync(Guid classId)
        {
            var @class = await GetByIdAsync(classId);
            if (@class == null || @class.StudentIds.Count == 0) return new();

            var allUsers = await _userRepository.GetAllAsync();

            return allUsers
                .Where(u => @class.StudentIds.Contains(ToGuid(u.Id)))
                .ToList();
        }

        public async Task AddStudentsToClassAsync(Guid classId, List<Guid> userIds)
        {
            var @class = await GetByIdAsync(classId);
            if (@class == null) return;

            foreach (var id in userIds)
            {
                if (!@class.StudentIds.Contains(id) && @class.StudentIds.Count < 25)
                {
                    @class.StudentIds.Add(id);
                }
            }

            await UpdateAsync(@class);
        }

        public static Guid ToGuid(ObjectId objectId)
        {
            var bytes = objectId.ToByteArray();

            var padded = new byte[16];
            Array.Copy(bytes, padded, Math.Min(12, bytes.Length));

            return new Guid(padded);
        }
    }
}