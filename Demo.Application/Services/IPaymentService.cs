using Demo.Core.Models;
using Demo.Core.ValueObjects;

namespace Demo.Application.Services
{
    public interface IPaymentService
    {
        List<Payment> GetPaid(Guid orderId, PaymentState deliveryState);
    }
}
