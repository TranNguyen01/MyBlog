using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlog.Models
{
    public class DocumentCollection
    {
        public Guid CollectionsId { get; set; }

        [NotMapped]
        public virtual Collections Collection { get; set; }

        public Guid DocumentId { get; set; }

        [NotMapped]
        public virtual Document Document { get; set; }

        public DateTime CreateAt { get; set; }
    }
}
