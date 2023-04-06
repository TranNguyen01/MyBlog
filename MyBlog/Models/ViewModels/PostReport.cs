using System;

namespace MyBlog.Models.ViewModels
{
    public class PostReport
    {
        public string UserId { get; set; }
        public Guid PostId { get; set; }
        public Post Post { get; set; }
        public int Count { get; set; }
    }
}
