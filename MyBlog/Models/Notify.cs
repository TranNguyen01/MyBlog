using System;
using System.Collections.Generic;

namespace MyBlog.Models
{
    public class Notify
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public int Type { get; set; }
        public string Title { get; set; }
        public string  Content { get; set; }
        public string Link { get; set; }
        public bool Seen { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
