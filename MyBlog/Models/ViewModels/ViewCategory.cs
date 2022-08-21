using System.ComponentModel.DataAnnotations;

namespace MyBlog.Models.ViewModels
{
    public class ViewCategory
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục là trường bắt buộc!")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên phải có độ dài nằm trong khoảng {1} đến {2} kí tự!")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Mô tả là trường bắt buộc!")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "ID Danh mục cha")]
        public int? ParentCategoryId { get; set; }
    }
}
