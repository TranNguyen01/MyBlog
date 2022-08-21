using System;
using System.ComponentModel.DataAnnotations;

namespace MyBlog.Models.ViewModels
{
    public class ViewSetting
    {
        [Required(ErrorMessage = "Email là trường bắt buộc")]
        [Display(Name = "Email liên lạc")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại là trường bắt buộc")]
        [Display(Name = "Số điện thoại")]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Họ là trường bắt buộc")]
        [Display(Name = "Họ")]
        [DataType(DataType.Text)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Tên là trường bắt buộc")]
        [Display(Name = "Tên")]
        [DataType(DataType.Text)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Giới tính là trường bắt buộc")]
        [Display(Name = "Giới tính")]
        public bool Gender { get; set; }

        [Required(ErrorMessage = "Ngày sinh là trường bắt buộc")]
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        public int AvatarId { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public Photo Avatar { get; set; }

        [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc!")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurentPassword { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc!")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mói")]
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Mật khẩu không trùng khớp!")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]

        public string ConfirmPassword { get; set; }
    }
}
