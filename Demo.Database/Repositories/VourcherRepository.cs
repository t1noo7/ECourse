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

        public async Task UpdateQuantity(Guid id, int quantity)
        {
            var filter = Builders<Voucher>.Filter.Eq(x => x.Id, id);

            var update = Builders<Voucher>.Update.Set(x => x.Quantity, quantity);

            var result = await _collection.FindOneAndUpdateAsync(filter, update,
                options: new FindOneAndUpdateOptions<Voucher> { ReturnDocument = ReturnDocument.After }
            );
        }

        public async Task<(bool isValid, string message, Voucher? voucher, long finalPrice)> ValidateVoucher(string code, long originalPrice)
        {
            if (string.IsNullOrWhiteSpace(code))
                return (false, "Mã giảm giá trống!", null, originalPrice);

            var voucher = await _collection.Find(x => x.Code == code.Trim()).FirstOrDefaultAsync();
            if (voucher == null || voucher.StartDate.Date > DateTimeExtensions.UTCNowVN.Date)
                return (false, $"Mã giảm giá {code} không tồn tại!", null, originalPrice);

            if (voucher.Expired.Date < DateTimeExtensions.UTCNowVN.Date)
                return (false, $"Mã giảm giá {code} đã hết hạn!", null, originalPrice);

            if (voucher.Quantity <= 0)
                return (false, $"Mã giảm giá {code} đã hết lượt sử dụng!", null, originalPrice);

            long discountedPrice = originalPrice;

            if (voucher.DiscountRate > 0)
                discountedPrice = originalPrice * (100 - voucher.DiscountRate) / 100;
            else if (voucher.DiscountAmount > 0)
                discountedPrice = originalPrice - voucher.DiscountAmount;

            discountedPrice = Math.Max(discountedPrice, 0);

            return (true, "", voucher, discountedPrice);
        }

        public async Task AddUsedOrderId(Guid voucherId, Guid orderId)
        {
            var update = Builders<Voucher>.Update.AddToSet(v => v.UsedOrderIds, orderId);
            await _collection.UpdateOneAsync(v => v.Id == voucherId, update);
        }
    }
}
