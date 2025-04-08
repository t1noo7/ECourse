namespace Demo.Application.ViewModels
{
    public class ApprovedStudentViewModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid RegisteredCourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
    }
}