using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace Demo.Core.Models
{
    public class Category : BaseEntity
    {
        [Display(Name = "Tên Danh Mục")]
        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        public string CatName { get; set; }

        [Display(Name = "Miêu Tả Danh Mục")]
        public string? Description { get; set; }

        [Display(Name = "Trạng Thái Hoạt Động")]
        public bool Status { get; set; }

        [Display(Name = "Ảnh")]
        public string? Image { get; set; }

        [Display(Name = "Tên Thay Thế")]
        public string? Alias { get; set; }

        [Display(Name = "Danh Mục Con")]
        [BsonElement("SubCat")]
        public List<SubCat>? SubCat { get; set; }

        [Display(Name = "Đường Dẫn")]
        public string? LinkAddress { get; set; }
    }

    public class SubCat
    {
        [Display(Name = "Tên Danh Mục Con")]
        [Required(ErrorMessage = "Tên danh mục con không được để trống")]
        public string? SubCatName { get; set; }

        [Display(Name = "Đường Dẫn")]
        public string? LinkAddress { get; set; }

        [Display(Name = "Trạng Thái Hoạt Động")]
        public bool Status { get; set; }
    }
}