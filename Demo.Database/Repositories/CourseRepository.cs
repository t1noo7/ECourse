using MongoDB.Driver;
using Demo.Application.Repositories;
using Demo.Core.Models;

namespace Demo.Database.Repositories
{
    public class CourseRepository : BaseRepository<Course>, ICourseRepository
    {
        public CourseRepository(IMongoDatabase db) : base(db)
        {
        }
    }
}
