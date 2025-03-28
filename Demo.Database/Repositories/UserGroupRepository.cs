using MongoDB.Driver;
using Demo.Core.Permission;
using Demo.Core.Repositories;

namespace Demo.Database.Repositories
{
    public class UserGroupRepository : BaseRepository<UserGroup>, IUserGroupRepository
    {
        public UserGroupRepository(IMongoDatabase db) : base(db)
        {
        }
    }
}
