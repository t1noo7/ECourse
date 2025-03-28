using System.ComponentModel.DataAnnotations;

namespace Demo.Core.Models
{
    public class New : BaseEntity
    {
        // public int ID{ get; set; }
        [Display(Name = "Tiêu Đề")]
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string Title { get; set; }

        [Display(Name = "Nội Dung")]
        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; }

        [Display(Name = "Ảnh")]
        public string? Image { get; set; }

        [Display(Name = "Trạng Thái Hoạt Động")]
        public bool Status { get; set; }

        /*[Display(Name = "Tên Thay Thế")]
        public string? Alias { get; set; }*/

        [Display(Name = "Tác Giả")]
        public string? Author { get; set; }

        /*[Display(Name = "Nhãn")]
        public string? Tags { get; set; }*/

        [Display(Name = "Lượt Xem")]
        public int? Views { get; set; }
    }
}