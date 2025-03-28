using MongoDB.Bson;
using MongoDB.Driver;
using Demo.Application.Models;
using Demo.Application.Repositories;
using Demo.Core.Models;

namespace Demo.Database.Repositories
{
    public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(IMongoDatabase db) : base(db)
        {
        }

        public Task<List<Payment>> FindAsync(PaymentFilter filter)
        {
            var builder = Builders<Payment>.Filter;
            FilterDefinition<Payment> filterDefinition = Builders<Payment>.Filter.Empty;

            if (!string.IsNullOrWhiteSpace(filter.query))
            {
                filterDefinition &= builder.Or(builder.Regex(m => m.CustomerName, new BsonRegularExpression($"{filter.query}", "i")) |
                    builder.Regex(m => m.CustomerPhone, new BsonRegularExpression($"{filter.query}", "i")));
            }

            return _collection.Find(filterDefinition).ToListAsync();
        }
    }
}
