using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyBlog.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Thể loại")]
        public int? CategoryId { get; set; }

        [Display(Name = "Thể loại")]
        public Category Category { get; set; }


        [Display(Name = "Tác giả")]
        public string? AuthorId { get; set; }

        [Display(Name = "Tác giả")]
        public User Author { get; set; }


        [Required(ErrorMessage = "Tên bài viết là bắt buộc!")]
        [Display(Name = "Tên bài viết")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên phải có độ dài trong khoảng từ {1} đến {2} kí tự!")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Địa chỉ bài viết là bắt buộc!")]
        [Display(Name = "Địa chỉ bài viết")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Địa chỉ bài viết phải có độ dài trong khoảng {1} đến {2} kí tự!")]
        public string Slug { get; set; }

        [Display(Name = "Ảnh bìa")]
        public int ThumbnailId { get; set; }

        [Display(Name = "Ảnh bìa")]
        public Photo Thumbnail { get; set; }

        [Required(ErrorMessage = "Tóm tắt là bắt buộc!")]
        [Display(Name = "Tóm tắt")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Nội dung bài viết là bắt buộc!")]
        [Display(Name = "Nội dung")]
        public string Content { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; }

        [DataType(DataType.Time)]
        public DateTime LastUpdatedAt { get; set; }


        [Display(Name = "Yêu thích")]
        public int LikesCount { get; set; }

        [Display(Name = "Bình luận")]
        public ICollection<Comment> Comments { get; set; }

        [Display(Name = "Yêu thích")]
        public ICollection<Like> Likes { get; set; }
    }
}
