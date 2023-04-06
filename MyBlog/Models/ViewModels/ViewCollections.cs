using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyBlog.Models.ViewModels
{
    public class ViewCollections
    {
        [Key]
        public string Id { get; set; }

        [Required(ErrorMessage = "Tên là bắt buộc")]
        [StringLength(maximumLength: 200, MinimumLength = 1, ErrorMessage = "Tên phải có độ dài từ {1} đên {0} kí tự")]
        [Display(Name = "Tên thư mục")]
        public string Name { get; set; }

        [Display(Name = "Người dùng")]
        public string UserId { get; set; }
        public virtual User User { get; set; }

        [Display(Name = "Thư mục cha")]
        public string? ParentCollectionsId { get; set; }
        public Collections ParentCollections { get; set; }

        [Display(Name = "Thư mục con")]
        public ICollection<Collections> ChildrenCollections { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreateAt { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime UpdateAt { get; set; }

        [Display(Name = "Danh sách bài viết")]
        public ICollection<PostCollection> PostCollections { get; set; }
    }
}
