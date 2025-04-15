using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Demo.Web.Areas.Admin.Models
{
    public class ClassViewModel
    {
        public Guid Id { get; set; }
        public string ClassName { get; set; }
        public string CourseName { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public List<ObjectId> StudentIds { get; set; } = new();
        public DateTime Created {  get; set; }
    }
}
