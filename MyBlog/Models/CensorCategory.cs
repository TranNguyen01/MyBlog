using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlog.Models
{
    public class CensorCategory
    {
        public string UserId { get; set; }
        public Guid CategoryId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int status { get; set; }

        [NotMapped]
        public Category Category { get; set; }
        [NotMapped]
        public User User { get; set; }
    }
}
