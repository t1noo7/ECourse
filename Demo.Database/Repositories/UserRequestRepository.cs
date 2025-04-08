using Demo.Application.Repositories;
using Demo.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Database.Repositories
{
    public class UserRequestRepository : BaseRepository<UserRequest>, IUserRequestRepository
    {
        public UserRequestRepository(IMongoDatabase db) : base(db)
        {
        }
    }
}
