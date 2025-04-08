using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Core.Models
{
    public class UserRequest : BaseEntity
    {
        [Display(Name = "Tên")]
        [Required(ErrorMessage = "Tên không được để trống")]
        public string FullName { get; set; }

        [Display(Name = "Tuổi")]
        public string? Age { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Email không được để trống")]
        public string Email { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Display(Name = "Số điện thoại")]
        [RegularExpression("(84|0[3|5|7|8|9]|0\\d{2})+([0-9]{8})$", ErrorMessage = "Số điện thoại không đúng định dạng")]
        public string Phone { get; set; }

        [Display(Name = "Lĩnh vực kinh doanh")]
        public string? BusinessField { get; set; }

        [Display(Name = "Khó khăn gặp phải")]
        [Required(ErrorMessage = "Khó khăn gặp phải không được để trống")]
        public string Difficulty { get; set; }
    }
}
