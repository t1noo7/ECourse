using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace Demo.Core.Models
{
    public class Categories : BaseEntity
    {
        [Display(Name = "Tên danh mục")]
        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        public string? CatName { get; set; }

        [Display(Name = "Miêu tả danh mục")]
        public string? Description { get; set; }

        [Display(Name = "Công khai")]
        public bool Published { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string? Thumb { get; set; }

        [Display(Name = "Tên thay thế")]
        public string? Alias { get; set; }

        [Display(Name = "Danh mục con")]
        [BsonElement("SubCat")]
        public List<SubCat>? SubCat { get; set; }

        [Display(Name = "Đường dẫn")]
        public string? LinkAddress { get; set; }
    }

    public class SubCat
    {
        [BsonElement("name")]
        public string? Name { get; set; }

        [BsonElement("link")]
        public string? Link { get; set; }

        [BsonElement("order")]
        public int Order { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; }
    }
}