using System.ComponentModel.DataAnnotations;

namespace Demo.Core.Models
{
    public class Lesson : BaseEntity
    {
        [Display(Name = "Tiêu đề")]
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string Title { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Chương")]
        public Guid ChapterId { get; set; }

        [Display(Name = "Video")]
        [Required(ErrorMessage = "Video không được để trống")]
        public string YouTubeUrl { get; set; }

        [Display(Name = "Nội dung")]
        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; }
    }
}