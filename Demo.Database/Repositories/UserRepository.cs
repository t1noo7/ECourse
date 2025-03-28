using MongoDB.Bson;
using MongoDB.Driver;
using Demo.Core.Permission;
using Demo.Core.Repositories;
using Demo.Core.Models;

namespace Demo.Database.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(IMongoDatabase db) : base(db)
        {
        }

        public Task<List<User>> FindAsync(FilterModel filter)
        {
            var builder = Builders<User>.Filter;
            var filterBuilder = builder.Regex(m => m.FullName, new BsonRegularExpression($"{filter.query}", "i")) |
                builder.Regex(m => m.Email, new BsonRegularExpression($"{filter.query}", "i")) |
                builder.Regex(m => m.UserName, new BsonRegularExpression($"{filter.query}", "i")) |
                builder.Regex(m => m.PhoneNumber, new BsonRegularExpression($"{filter.query}", "i"));

            if (!string.IsNullOrWhiteSpace(filter.custom))
            {
                if (filter.custom == "Nhân viên")
                {
                    filterBuilder &= builder.SizeGt(m => m.CustomRoles, 0);
                }
                else
                    filterBuilder &= builder.Size(m => m.CustomRoles, 0);
            }

            return _collection.Find(filterBuilder).ToListAsync();
        }

        public Task<User> GetByIdAsync(string id)
        {
            return _collection.Find(Builders<User>.Filter.Eq(m => m.Id, ObjectId.Parse(id))).FirstOrDefaultAsync();
        }

        public User GetByUsername(string username) => _collection.Find(Builders<User>.Filter.Eq(m => m.UserName, username)).FirstOrDefault();

        public override async Task<User> UpdateAsync(User model)
        {
            var filter = Builders<User>.Filter.Where(x => x.Id == model.Id);
            var options = new FindOneAndReplaceOptions<User, User>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };
            var updatedEntity = await _collection.FindOneAndReplaceAsync(filter, model, options);
            return model;
        }
    }
}
