using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace MyBlog.Models
{
    [ElasticsearchType(Name = "Document")]
    public class Document: BaseModel
    {
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public string BucketName { get; set; }
        public string OriginFileName { get; set; }
        public long Length { get; set; }
        public string? AuthorID { get; set; }
        public Guid? CategoryId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Deleted { get; set; }
        public int Dowloaded { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Status { get; set; }

        [Ignore]
        [NotMapped]
        public virtual Category Category { get; set; }
        [Ignore]
        [NotMapped]
        public virtual User Author { get; set; }
        public BaseModel ExportBaseModel()
        {
            return new BaseModel
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
            };
        }

        public BaseESModel ExportES()
        {
            return new BaseESModel
            {
                Id = new Guid(),
                Name = this.Name,
                Description = this.Description,
                Content = ""
            };
        }
    }
}
