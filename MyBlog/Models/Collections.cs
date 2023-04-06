using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MyBlog.Models
{
    public class Collections
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(maximumLength: 200, MinimumLength = 1, ErrorMessage = "Tên phải có độ dài từ {1} đên {0} kí tự")]
        public string Name { get; set; }

        [Required]
        public string UserId { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual User User { get; set; }

        public Guid? ParentCollectionsId { get; set; }
        public virtual Collections ParentCollections { get; set; }

        public ICollection<Collections> ChildrenCollections { get; set; }

        [Required]
        public DateTime CreateAt { get; set; }

        [Required]
        public DateTime UpdateAt { get; set; }

        [NotMapped]
        public virtual ICollection<PostCollection> PostCollections { get; set; }

        [NotMapped]
        public virtual ICollection<DocumentCollection> DocumentCollections { get; set; }
    }
}
