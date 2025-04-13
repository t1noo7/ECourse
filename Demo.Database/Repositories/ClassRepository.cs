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
            var orders = await _orderRepository.GetAllAsync();

            // 1. Lọc tất cả đơn hàng đã được duyệt
            var approvedOrders = orders
                .Where(o => o.Status == OrderStatus.Approved &&
                            (!string.IsNullOrWhiteSpace(o.CustomerEmail) ||
                             !string.IsNullOrWhiteSpace(o.CustomerName) ||
                             !string.IsNullOrWhiteSpace(o.CustomerPhone)))
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

            // 3. Lấy tất cả lớp và studentIds đã được xếp
            var allClasses = await GetAllAsync();
            var studentCoursePairs = allClasses
                .SelectMany(c => c.StudentIds.Select(sid => new { sid, courseId = c.CourseId }))
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

                // ❗ Kiểm tra nếu học viên này đã được xếp lớp cho đúng khóa học này chưa
                if (studentCoursePairs.Contains(new { sid = user.Id, courseId = order.Course.Id }))
                {
                    continue;
                }

                students.Add(new ApprovedStudentViewModel
                {
                    Id = user.Id,
                    FullName = order.CustomerName,
                    Email = user.Email,
                    PhoneNumber = order.CustomerPhone,
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

            var courseId = @class.CourseId;

            var users = await _userRepository.GetAllAsync();
            var orders = await _orderRepository.GetAllAsync();

            var students = users
                .Where(u => @class.StudentIds.Contains(u.Id))
                .Select(u =>
                {
                    // Match đơn hàng theo Email và đúng khoá học của lớp
                    var order = orders
                        .Where(o =>
                            o.CustomerEmail?.ToLower() == u.Email?.ToLower() &&
                            o.Course?.Id == courseId &&
                            o.Status == OrderStatus.Approved)
                        .OrderByDescending(o => o.Created)
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