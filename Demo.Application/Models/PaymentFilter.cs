using Demo.Core.Enums;
using Demo.Core.Permission;

namespace Demo.Application.Models
{
    public class PaymentFilter : FilterModel
    {
        public PaymentState? State { get; set; }
    }
}