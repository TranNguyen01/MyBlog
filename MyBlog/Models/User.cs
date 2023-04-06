using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyBlog.Models
{
    public class User : IdentityUser
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string FirstName { get; set; }

        [Required]
        public bool Gender { get; set; }

        public DateTime CreatedAt { get; set; }
        public Guid? AvatarId { get; set; }

        public Photo Avatar { get; set; }

        public ICollection<Like> Likes { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Post> Posts { get; set; }
        public ICollection<Document> Documents { get; set; }
        public ICollection<Collections> Collections { get; set; }


    }
}
