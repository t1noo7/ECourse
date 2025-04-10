using System.ComponentModel.DataAnnotations;
using Demo.Common.Extensions;
using Demo.Core.Enums;

namespace Demo.Core.Models
{
    public class Order : BaseEntity
    {
        public string? Code { get; set; }
        public string? CustomerNote { get; set; }
        public string? AdminNote { get; set; }

        [Required(ErrorMessage = "Giá không được để trống")]
        public long Price { get; set; }

        public string? Username { get; set; }

        [Required(ErrorMessage = "Tên khách hàng không được để trống")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Email khách hàng không được để trống")]
        public string CustomerEmail { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        public string? CustomerPhone { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        public string? CustomerAddress { get; set; }

        [Required(ErrorMessage = "Ảnh xác thực chuyển khoản không được để trống")]
        public string? VerifyImageUrl { get; set; }

        public List<OrderStatusDetails> StatusHistories { get; set; } = new List<OrderStatusDetails>();
        public OrderStatus Status { get; set; }

        [Required(ErrorMessage = "Nội dung thanh toán không được để trống")]
        public string PaymentContent { get; set; }
        public Course Course { get; set; }

        public List<Guid> CourseIds { get; set; } = new List<Guid>();
        public Voucher Voucher { get; set; } = new Voucher();
        public PaymentOption PaymentOption { get; set; }

        public static IEnumerable<(OrderStatus value, string text)> GetPossibleStatuses()
        {
            var values = Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>();
            foreach (var item in values)
            {
                yield return new(item, item.GetEnumDescription());
            }
        }
    }
}