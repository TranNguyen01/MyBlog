using System.ComponentModel.DataAnnotations;

namespace MyBlog.Models.ViewModels
{
    public class ViewPost
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên bài viết là bắt buộc!")]
        [Display(Name = "Tên bài viết")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên phải có độ dài trong khoảng từ {1} đến {2} kí tự!")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Tóm tắt là bắt buộc!")]
        [Display(Name = "Tóm tắt")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Nội dung bài viết là bắt buộc!")]
        [Display(Name = "Nội dung")]
        public string Content { get; set; }

        //[Required(ErrorMessage = "Ảnh bìa là bắt buộc!")]
        [Display(Name = "Ảnh bìa")]
        public string Thumbnail { get; set; }

        [Required(ErrorMessage = "Thể loại là bắt buộc!")]
        [Display(Name = "Thể loại")]
        public int CategoryId { get; set; }

        [Display(Name = "Thể loại")]
        public Category Category { get; set; }

        [Display(Name = "Thể loại")]
        public int AuthorId { get; set; }

        public User Author { get; set; }

    }
}
