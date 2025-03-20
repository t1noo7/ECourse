using Demo.Application.Repositories;
using Demo.Core.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Database.Repositories
{
    public class NewRepository : BaseRepository<New>, INewRepository
    {
        public NewRepository(IMongoDatabase db) : base(db)
        {
        }
    }
}
