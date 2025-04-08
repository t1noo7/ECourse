using System.ComponentModel.DataAnnotations;

namespace Demo.Core.Models
{
    public class Class : BaseEntity
    {
        [Display(Name = "Tên lớp")]
        [Required(ErrorMessage = "Tên lớp không được để trống")]
        public string ClassName { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }
        public Course Course { get; set; }

        [Display(Name = "Nội dung")]
        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; }

        [Display(Name = "Học viên")]
        public List<Guid> StudentIds { get; set; } = new();
    }
}