using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Core.Models
{
    public class Banner : BaseEntity
    {
        [Display(Name = "Tiêu đề banner")]
        [Required(ErrorMessage = "Tiêu đề banner không được để trống")]
        public string Title { get; set; }

        [Display(Name = "Nội dung banner")]
        public string? Content { get; set; }

        [Display(Name = "Nội dung button")]
        public string? ButtonContent { get; set; }

        [Display(Name = "Ảnh")]
        [Required(ErrorMessage = "Ảnh không được để trống")]
        public string Image { get; set; }

        [Display(Name = "Thứ tự")]
        public int Order { get; set; }

        [Display(Name = "Trạng thái hoạt động")]
        public bool Status { get; set; }
    }
}
