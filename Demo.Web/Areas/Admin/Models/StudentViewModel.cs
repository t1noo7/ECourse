using Demo.Core.Models;

namespace Demo.Web.Areas.Admin.Models
{
    public class StudentViewModel : User
    {
        public List<Course> Course { get; set; }
        public List<Class> Class { get; set; }
    }
}
