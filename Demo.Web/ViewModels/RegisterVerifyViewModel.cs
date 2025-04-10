using System.ComponentModel.DataAnnotations;

namespace Demo.Web.ViewModels
{
    public class RegisterVerifyViewModel
    {
        public string Email { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã xác minh gồm 6 chữ số.")]
        public string Code { get; set; }
    }
}
