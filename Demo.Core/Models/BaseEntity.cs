using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Demo.Core.Models
{
    public class BaseEntity
    {
        [BsonId(IdGenerator = typeof(CombGuidGenerator))]
        public Guid Id { get; set; }

        public string? FriendlyUrl { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }

        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
        public bool Deleted { get; set; }
    }
}
