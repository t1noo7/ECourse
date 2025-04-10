using Demo.Core.Enums;
using Demo.Core.Models;
using Demo.Core.Permission;

namespace Demo.Application.Models
{
    public class OrderFilter : FilterModel
    {
        public OrderStatus? OrderStatus { get; set; }
        public PaymentState? PaymentState { get; set; }
        public string? Code { get; set; }
        public string? courses { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
}
