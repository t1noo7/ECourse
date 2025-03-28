using Demo.Application.Models;
using Demo.Core.Models;
using Demo.Core.Repositories;
using Demo.Core.ValueObjects;

namespace Demo.Application.Repositories
{
    public interface IOrderRepository : IBaseRepository<Order>
    {
        Task<List<Order>> FindAsync(OrderFilter filter);
        // Task UpdatePaymentStatus(Guid id, Guid courseId, PaymentStatus status);
    }
}
