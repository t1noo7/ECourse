using Demo.Application.Repositories;
using Demo.Core.Models;
using MongoDB.Driver;

namespace Demo.Database.Repositories
{
    public class ChapterRepository : BaseRepository<Chapter>, IChapterRepository
    {
        public ChapterRepository(IMongoDatabase db) : base(db)
        {
        }
    }
}