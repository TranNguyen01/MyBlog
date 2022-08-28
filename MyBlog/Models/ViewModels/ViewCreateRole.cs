using System.ComponentModel.DataAnnotations;

namespace MyBlog.Models.ViewModels
{
    public class ViewCreateRole
    {
        [Display(Name = "Tên vai trò")]
        [Required]
        public string Name { get; set; }
    }
}
