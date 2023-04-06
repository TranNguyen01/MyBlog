using System;

namespace MyBlog.Models.ViewModels
{
    public class ViewCensorship
    {
        public string Id { get; set; }
        public string PostId { get; set; }
        public Post Post { get; set; }
        public string DocumentId { get; set; }
        public Document Document { get; set; }
        public string UserId { get; set; }
        public int Status { get; set; }
        public string ReasonId { get; set; }
        public Reason Reason { get; set; }
        public string Comment { get; set; }
        public string Reply { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
