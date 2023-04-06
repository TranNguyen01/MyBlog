using MyBlog.Models.ViewModels;
using Nest;
using System;

namespace MyBlog.Models
{
    public class Report
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public Guid? PostId { get; set; }
        public Post Post { get; set; }
        public Guid? DocumentId { get; set; }
        public Document Document { get; set; }
        public Guid ReasonId { get; set; }
        public Reason Reason { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Reviewed { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public void Parse(ViewReport obj)
        {
            if (!string.IsNullOrEmpty(obj.Id))
                Id = Guid.Parse(obj.Id);
            if (!string.IsNullOrEmpty(obj.UserId))
                UserId = obj.UserId;
            if (!string.IsNullOrEmpty(obj.PostId))
                PostId = Guid.Parse(obj.PostId);
            if (!string.IsNullOrEmpty(obj.DocumentId))
                DocumentId = Guid.Parse(obj.DocumentId);
            if (!string.IsNullOrEmpty(obj.ReasonId))
                ReasonId = Guid.Parse(obj.ReasonId);
            Content = obj.Content;
            Reviewed = obj.Reviewed;
        }
    }
}
