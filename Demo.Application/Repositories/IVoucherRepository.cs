using Demo.Core.Models;
using Demo.Core.Repositories;

namespace Demo.Application.Repositories
{
    public interface IVoucherRepository : IBaseRepository<Voucher>
    {
        Task UpdateQuantity(Guid id, int quantity);
        Task<(bool isValid, string message, Voucher? voucher, long finalPrice)> ValidateVoucher(string code, long originalPrice);
    }
}