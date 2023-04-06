using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlog.Models
{
    public class PostCollection
    {
        public Guid CollectionsId { get; set; }

        [NotMapped]
        public virtual Collections Collections { get; set; }

        public Guid PostId { get; set; }

        [NotMapped]
        public virtual Post Post { get; set; }

        public DateTime CreateAt { get; set; }
    }
}
