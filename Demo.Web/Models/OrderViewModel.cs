using Microsoft.AspNetCore.Mvc.Rendering;
using Demo.Web.Helpers;
using System.ComponentModel.DataAnnotations;
using Demo.Core.Enums;

namespace Demo.Web.Models
{
    public class OrderViewModel
    {

        [Required(ErrorMessage = "Tên không được để trống")]
        public string? CustomerName { get; set; }
        [Required(ErrorMessage = "Email không được để trống")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail không đúng định dạng")]
        public string? CustomerEmail { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression("(84|0[3|5|7|8|9]|0\\d{2})+([0-9]{8})$", ErrorMessage = "Số điện thoại không đúng định dạng")]
        public string? CustomerPhone { get; set; }
        public string? CustomerNote { get; set; }

        public Guid CourseId { get; set; }

        // [Required(ErrorMessage = "Ảnh chụp màn hình thanh toán không được để trống")]
        public string? VerifyImageUrl { get; set; }

        /// <summary>
        /// Mã voucher
        /// </summary>
        public string? VoucherCode { get; set; }

        /// <summary>
        /// Hình thức thanh toán
        /// </summary>
        [Required(ErrorMessage = "Phải chọn hình thức thanh toán")]
        [Range(1, 2, ErrorMessage = "Phải chọn hình thức thanh toán")]
        public int PaymentOption { get; set; }

        public IEnumerable<SelectListItem> PaymentOptions
        {
            get
            {
                return EnumHelper.EnumToListItems<PaymentOption>();
            }
        }
    }
}
