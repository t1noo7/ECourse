using Demo.Core.Models;
using Demo.Application.ViewModels;

namespace Demo.Web.Areas.Admin.Models
{
    public class AssignStudentsViewModel
    {
        public List<Course> AvailableCourses { get; set; } = new();
        public List<ApprovedStudentViewModel> ApprovedStudents { get; set; } = new();
    }
}