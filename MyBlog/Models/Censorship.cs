using MyBlog.Models.ViewModels;
using Nest;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlog.Models
{
    public class Censorship
    {
        public Guid Id { get; set; }
        public Guid? PostId { get; set; }
        [NotMapped]
        public Post Post { get; set; }
        public Guid? DocumentId { get; set; }
        [NotMapped]
        public Document Document { get; set; }
        public string UserId { get; set; }
        public int Status { get; set; }
        public Guid? ReasonId { get; set; }
        [NotMapped]
        public Reason Reason { get; set; }
        public string Comment { get; set; }
        public string Reply { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Censorship()
        {
        }
        public Censorship(ViewCensorship obj)
        {
            Parse(obj);
        }

        public void Parse(ViewCensorship obj)
        {
            if (!string.IsNullOrEmpty(obj.Id))
                Id = Guid.Parse(obj.Id);
            if (!string.IsNullOrEmpty(obj.PostId))
                PostId = Guid.Parse(obj.PostId);
            if (!string.IsNullOrEmpty(obj.DocumentId))
                DocumentId = Guid.Parse(obj.DocumentId);
            if (!string.IsNullOrEmpty(obj.UserId))
                UserId = obj.UserId;
            if (!string.IsNullOrEmpty(obj.ReasonId))
                ReasonId = Guid.Parse(obj.ReasonId);
            Status = obj.Status;
            Comment = obj.Comment;
            Reply = obj.Reply;
        }

        public ViewCensorship Export()
        {
            return new ViewCensorship
            {
                Id = Id.ToString(),
                PostId = PostId?.ToString(),
                Post = Post,
                DocumentId = DocumentId?.ToString(),
                Document = Document,
                UserId = UserId,
                Status = Status,
                ReasonId = ReasonId?.ToString(),
                Reason = Reason,
                Comment = Comment,
                Reply = Reply,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt
            };
        }
    }
}
