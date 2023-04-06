using System;
using System.ComponentModel.DataAnnotations;

namespace MyBlog.Models.ViewModels
{
    public class ViewUpdateDocument
    {
        [Display(Name = "Id tài liệu")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Tên tài liệu là trường bắt buộc!")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên phải có độ dài nằm trong khoảng {1} đến {2} kí tự!")]
        [Display(Name = "Tên tài liệu")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Danh mục tài liệu là trường bắt buộc!")]
        [Display(Name = "Danh mục tài liệu")]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "Mô tả trường bắt buộc!")]
        [Display(Name = "Mô tả ngắn gọn")]
        public string Description { get; set; }

        public virtual Category Category { get; set; }
        public virtual User Author { get; set; }

        public void Parse(Document document)
        {
            Id = document.Id;
            Name = document.Name;
            CategoryId = (Guid)document.CategoryId;
            Description = document.Description;
            Category = document.Category;
            Author = document.Author;
        }
    }
}
