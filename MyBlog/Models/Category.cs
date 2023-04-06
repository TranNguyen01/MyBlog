using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlog.Models
{
    public class Category
    {
        [Key]
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục là trường bắt buộc!")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên phải có độ dài nằm trong khoảng {1} đến {2} kí tự!")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; }


        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Địa chỉ liên kết")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Địa chỉ phải có độ dài nằm trong khoảng {1} đến {2} kí tự")]
        [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "Chỉ dùng các ký tự [a-z0-9-]")]
        public string Slug { get; set; }

        [Display(Name = "ID Danh mục cha")]
        public Guid? ParentCategoryId { get; set; }

        //[ForeignKey("ParentCategoryId")]
        [Display(Name = "Danh mục cha")]
        public virtual Category ParentCategory { get; set; }

        public bool Deleted { get; set; }

        [Ignore]
        [Display(Name = "Danh mục con")]
        public virtual ICollection<Category> ChildrenCategory { get; set; }

        [Ignore]
        [Display(Name = "Bài viết")]
        [NotMapped]
        public virtual ICollection<Post> Posts { get; set; }

        [Ignore]
        [Display(Name = "Tài Liệu")]
        [NotMapped]
        public virtual ICollection<Document> Documents { get; set; }
    }
}
