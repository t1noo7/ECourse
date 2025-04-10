using Demo.Core.Enums;
using Demo.Core.Models;

namespace Demo.Application.Services.IServices
{
    public interface IPaymentService
    {
        List<Payment> GetPaid(Guid orderId, PaymentState deliveryState);
    }
}
