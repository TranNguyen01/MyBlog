using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlog.Models
{
    public class Photo
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PublicId { get; set; }
        public int Version { get; set; }
        public string Signature { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Format { get; set; }
        public string ResourceType { get; set; }
        public int Bytes { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public string SecureUrl { get; set; }
        public string Path { get; set; }

        public Guid? PostId { get; set; }

        [NotMapped]
        public Post Post { get; set; }

        public string UserId { get; set; }
        
        [NotMapped]
        public User? User { get; set; }
    }
}
