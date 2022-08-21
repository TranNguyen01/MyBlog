using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlog.Models.ViewModels
{
    public class ViewComment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Bài viết")]
        public int PostId { get; set; }


        [Display(Name = "Bài viết")]
        public Post Post { get; set; }

        [Display(Name = "Người dùng")]
        public string UserId { get; set; }

        [ForeignKey("Người dùng")]
        public User User { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Nội dung chỉ giới hạn trong 1000 kí tự!")]
        public string Content { get; set; }

        [DataType(DataType.Time)]
        public DateTime CreatedAt { get; set; }
    }
}
