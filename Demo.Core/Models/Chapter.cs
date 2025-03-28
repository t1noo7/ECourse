using Demo.Common.Extensions;
using Demo.Core.Enums;
using Demo.Core.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace Demo.Core.Models
{
    public class Chapter : BaseEntity
    {
        [Display(Name = "Tiêu Đề")]
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string Title { get; set; }

        public Guid CourseId { get; set; }

        [Display(Name = "Thứ Tự")]
        public int Order { get; set; }
    }
}