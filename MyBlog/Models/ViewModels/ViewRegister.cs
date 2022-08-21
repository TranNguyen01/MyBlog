using System;
using System.ComponentModel.DataAnnotations;

namespace MyBlog.Models.ViewModels
{
    public class ViewRegister
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress]
        [Display(Name = "Đia chỉ email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Display(Name = "Số điện thoại")]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Họ là bắt buộc")]
        [Display(Name = "Họ")]
        [DataType(DataType.Text)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Tên là bắt buộc")]
        [Display(Name = "Tên")]
        [DataType(DataType.Text)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Giới là bắt buộc")]
        [Display(Name = "Giới tính")]
        public bool Gender { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất {2} kí tự")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu không trùng khớp!")]
        public string ConfirmPassword { get; set; }

    }
}
