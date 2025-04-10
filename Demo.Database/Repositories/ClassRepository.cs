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
                .Where(c => c.CourseId != null && c.CourseId == courseId && c.StudentIds.Count < 25)
                .ToList();
        }

        public async Task<List<ApprovedStudentViewModel>> GetApprovedStudentsWithCourseAsync()
        {
            // 1. Lấy đơn hàng đã duyệt
            var orders = await _orderRepository.GetAllAsync();
            var approvedOrders = orders
                .Where(o => o.Status == OrderStatus.Approved &&
                            (!string.IsNullOrWhiteSpace(o.CustomerEmail) ||
                             !string.IsNullOrWhiteSpace(o.CustomerName) ||
                             !string.IsNullOrWhiteSpace(o.CustomerPhone)))
                .GroupBy(o =>
                {
                    if (!string.IsNullOrWhiteSpace(o.CustomerEmail))
                        return o.CustomerEmail.Trim().ToLower();
                    if (!string.IsNullOrWhiteSpace(o.CustomerName))
                        return o.CustomerName.Trim().ToLower();
                    return o.CustomerPhone.Trim();
                })
                .Select(g => g.OrderByDescending(o => o.Created).First())
                .ToList();

            // 2. Lấy danh sách user
            var users = await _userRepository.GetAllAsync();

            var userDict = users
                .Where(u => !string.IsNullOrWhiteSpace(u.Email) ||
                            !string.IsNullOrWhiteSpace(u.UserName) ||
                            !string.IsNullOrWhiteSpace(u.PhoneNumber))
                .GroupBy(u =>
                {
                    if (!string.IsNullOrWhiteSpace(u.Email))
                        return u.Email.Trim().ToLower();
                    if (!string.IsNullOrWhiteSpace(u.UserName))
                        return u.UserName.Trim().ToLower();
                    return u.PhoneNumber.Trim();
                })
                .ToDictionary(g => g.Key, g => g.First());

            // 3. Lấy danh sách học viên đã được xếp lớp
            var allClasses = await GetAllAsync();
            var assignedStudentIds = allClasses
                .SelectMany(c => c.StudentIds)
                .ToHashSet();

            var students = new List<ApprovedStudentViewModel>();

            foreach (var order in approvedOrders)
            {
                string? key = null;

                if (!string.IsNullOrWhiteSpace(order.CustomerEmail))
                    key = order.CustomerEmail.Trim().ToLower();
                else if (!string.IsNullOrWhiteSpace(order.CustomerName))
                    key = order.CustomerName.Trim().ToLower();
                else if (!string.IsNullOrWhiteSpace(order.CustomerPhone))
                    key = order.CustomerPhone.Trim();

                if (string.IsNullOrEmpty(key))
                {
                    Console.WriteLine($"[WARN] Đơn hàng thiếu thông tin định danh: {order.Id}");
                    continue;
                }

                if (!userDict.TryGetValue(key, out var user))
                {
                    Console.WriteLine($"[WARN] Không tìm thấy user với key: {key}");
                    continue;
                }

                if (assignedStudentIds.Contains(user.Id)) continue;

                students.Add(new ApprovedStudentViewModel
                {
                    Id = user.Id,
                    FullName = order.CustomerName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    RegisteredCourseId = order.Course.Id,
                    CourseTitle = order.Course.Title
                });
            }

            return students;
        }

        public async Task<List<StudentInClassViewModel>> GetStudentsInClassAsync(Guid classId)
        {
            var @class = await GetByIdAsync(classId);
            if (@class == null) return new();

            var users = await _userRepository.GetAllAsync();
            var orders = await _orderRepository.GetAllAsync();

            var students = users
                .Where(u => @class.StudentIds.Contains(u.Id))
                .Select(u =>
                {
                    // Match order theo Email
                    var order = orders
                        .Where(o => o.CustomerEmail?.ToLower() == u.Email?.ToLower())
                        .OrderByDescending(o => o.Created) // Lấy đơn hàng mới nhất nếu có nhiều
                        .FirstOrDefault();

                    return new StudentInClassViewModel
                    {
                        Id = u.Id,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        CustomerName = order?.CustomerName ?? string.Empty,
                        Created = order?.Created ?? DateTime.MinValue
                    };
                })
                .ToList();

            return students;
        }

        public async Task AddStudentsToClassAsync(Guid classId, List<string> userIds)
        {
            var @class = await GetByIdAsync(classId);
            if (@class == null) return;

            foreach (var id in userIds)
            {
                var objectId = ObjectId.Parse(id);
                if (!@class.StudentIds.Contains(objectId) && @class.StudentIds.Count < 25)
                {
                    @class.StudentIds.Add(objectId);
                }
            }

            await UpdateAsync(@class);
        }
    }
}