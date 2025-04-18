using MongoDB.Driver;
using Demo.Application.Repositories;
using Demo.Core.Models;
using Demo.Common.Extensions;

namespace Demo.Database.Repositories
{
    public class VoucherRepository : BaseRepository<Voucher>, IVoucherRepository
    {
        public VoucherRepository(IMongoDatabase db) : base(db)
        {
        }
    }
}
