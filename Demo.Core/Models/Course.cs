using System.ComponentModel.DataAnnotations;

namespace Demo.Core.Models
{
    public class Course : BaseEntity
    {
        [Display(Name = "Tiêu đề")]
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string Title { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Ảnh")]
        public string? Image { get; set; }

        [Display(Name = "Trạng thái hoạt động")]
        public bool Status { get; set; }

        [Display(Name = "Giá")]
        [Required(ErrorMessage = "Giá không được để trống")]
        public int Price { get; set; }
        [Display(Name = "Giá đã giảm")]
        public int? DiscountPrice { get; set; }

        [Display(Name = "Thời lượng")]
        [Required(ErrorMessage = "Thời lượng không được để trống")]
        public int Time {  get; set; }

        /*[Display(Name = "Giáo viên")]
        [Required(ErrorMessage = "Giáo viên không được để trống")]
        public string Teacher { get; set; }*/
    }
}