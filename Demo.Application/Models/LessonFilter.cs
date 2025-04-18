using Demo.Core.Permission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Application.Models
{
    public class LessonFilter : FilterModel
    {
        public Guid CourseId { get; set; }
        public string CourseName {  get; set; }
        public string ClassName {  get; set; }
    }
}
