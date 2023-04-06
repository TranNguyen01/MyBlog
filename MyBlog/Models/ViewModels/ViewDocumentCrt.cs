using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MyBlog.Models.ViewModels
{
    public class ViewDocumentCrt
    {
        [Required(ErrorMessage = "Tên tài liệu là trường bắt buộc!")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên phải có độ dài nằm trong khoảng {1} đến {2} kí tự!")]
        [Display(Name = "Tên tài liệu")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Danh mục tài liệu là trường bắt buộc!")]
        [Display(Name = "Danh mục tài liệu")]
        public string CategoryId { get; set; }

        [Required(ErrorMessage = "Mô tả trường bắt buộc!")]
        [Display(Name = "Mô tả ngắn gọn")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Mô tả trường bắt buộc!")]
        [Display(Name = "File tài liệu")]
        public IFormFile File { get; set; }
    }
}
