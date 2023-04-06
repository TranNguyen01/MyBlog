using System;

namespace MyBlog.Models.ViewModels
{
    public class ViewDocument
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ContentType { get; set; }
        public string OriginFileName { get; set; }
        public long Length { get; set; }
        public string? AuthorID { get; set; }
        public Guid? CategoryId { get; set; }
        public string? TempUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual Category Category { get; set; }
        public virtual User Author { get; set; }

        public void Parse(Document document)
        {
            Id = document.Id;
            Name = document.Name;
            Description = document.Description;
            ContentType = document.ContentType;
            OriginFileName = document.OriginFileName;
            Length = document.Length;
            AuthorID = document.AuthorID;
            CategoryId = document.CategoryId;
            CreatedAt = document.CreatedAt;
            Category = document.Category;
            Author = document.Author;
        }
    }
}
