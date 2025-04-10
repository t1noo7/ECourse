// NOTE: Cả AssignStudentsViewModel và ApprovedStudentViewModel dùng ở tầng View + Repository bên Class
// Không tách ra file riêng để dễ quản lý (mục đích gán học viên vào lớp học)

using Demo.Core.Models;
using MongoDB.Bson;

namespace Demo.Application.ViewModels
{
    public class AssignStudentsViewModel
    {
        public List<Course> AvailableCourses { get; set; } = new();
        public List<ApprovedStudentViewModel> ApprovedStudents { get; set; } = new();
    }

    public class ApprovedStudentViewModel
    {
        public ObjectId Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public Guid RegisteredCourseId { get; set; }
        public string CourseTitle { get; set; }
    }

    public class StudentInClassViewModel
    {
        public ObjectId Id { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string CustomerName { get; set; }
        public DateTime Created { get; set; }
    }
}