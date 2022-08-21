using System.ComponentModel.DataAnnotations;

namespace MyBlog.Models.ViewModels
{
    public class ViewLogin
    {
        [Required(ErrorMessage = "Phải nhập {0}")]
        [Display(Name = "Tên đăng nhập")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phải nhập {0}")]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Lưu tài khoản")]
        public bool RememberMe { get; set; }
    }
}
