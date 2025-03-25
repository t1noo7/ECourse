using Demo.Core.Permission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Application.Models
{
    public class ChapterFilter : FilterModel
    {
        public string Course { get; set; }
        public bool CourseStatus { get; set; }
    }
}
