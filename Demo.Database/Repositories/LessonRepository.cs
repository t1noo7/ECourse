using Demo.Application.Repositories;
using Demo.Core.Models;
using MongoDB.Driver;

namespace Demo.Database.Repositories
{
    public class LessonRepository : BaseRepository<Lesson>, ILessonRepository
    {
        public LessonRepository(IMongoDatabase db) : base(db)
        {
        }
    }
}