using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlog.Models
{
    public class Comment
    {
        [Key]
        public Guid Id { get; set; }

        public string Slug { get; set; }

        [Required]
        [Display(Name = "Bài viết")]
        public Guid PostId { get; set; }

        [ForeignKey("PostId")]
        public Post Post { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Nội dung chỉ giới hạn trong 1000 kí tự!")]
        public string Content { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public DateTime CreatedAt { get; set; }
        public bool Deleted { get; set; }

    }
}
