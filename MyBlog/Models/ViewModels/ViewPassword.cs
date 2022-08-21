using System.ComponentModel.DataAnnotations;

namespace MyBlog.Models.ViewModels
{
    public class ViewPassword
    {
        [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc!")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurentPassword { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc!")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mói")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu không trùng khớp!")]
        public string ConfirmPassword { get; set; }
    }
}
