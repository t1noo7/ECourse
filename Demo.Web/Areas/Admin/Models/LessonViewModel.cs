using Demo.Core.Models;

namespace Demo.Web.Areas.Admin.Models
{
    public class LessonViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public Guid CourseId { get; set; }
        public string CourseName { get; set; }
        public Guid ClassId { get; set; }
        public string ClassName { get; set; }
        public DateTime Created { get; set; }
    }
}
