using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Demo.Core.Models
{
    public class Class : BaseEntity
    {
        [Display(Name = "Tên lớp")]
        [Required(ErrorMessage = "Tên lớp không được để trống")]
        public string ClassName { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }
        public Guid CourseId { get; set; }

        [Display(Name = "Nội dung")]
        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public List<ObjectId> StudentIds { get; set; } = new();
    }
}