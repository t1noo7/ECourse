using AspNetCore.Identity.Mongo.Model;
using Demo.Core.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace Demo.Core.Models
{
    [BsonIgnoreExtraElements]
    public class User : MongoUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }

        public DateTime? Updated { get; set; }
        public bool IsLocked { get; set; }

        public List<Guid> Groups { get; set; } = new List<Guid>();
        public List<string> CustomRoles { get; set; } = new List<string>();

        public DateTime? LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public List<Course> CartItem { get; set; } = new List<Course>();
    }
}