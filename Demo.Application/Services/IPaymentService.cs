using Demo.Core.Enums;
using Demo.Core.Models;

namespace Demo.Application.Services
{
    public interface IPaymentService
    {
        List<Payment> GetPaid(Guid orderId, PaymentState deliveryState);
    }
}
