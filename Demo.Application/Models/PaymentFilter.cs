using Demo.Core.Permission;
using Demo.Core.ValueObjects;

namespace Demo.Application.Models
{
    public class PaymentFilter : FilterModel
    {
        public PaymentState? State { get; set; }
    }
}