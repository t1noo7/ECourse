using Demo.Application.Models;
using Demo.Core.Models;
using Demo.Core.Repositories;

namespace Demo.Application.Repositories
{
    public interface IPaymentRepository : IBaseRepository<Payment>
    {
        Task<List<Payment>> FindAsync(PaymentFilter filter);
    }
}
