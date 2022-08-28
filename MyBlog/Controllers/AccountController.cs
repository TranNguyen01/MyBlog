
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyBlog.Models;
using MyBlog.Models.ViewModels;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly Cloudinary _Cloudinary;
        private readonly AppDbContext _Context;
        private readonly IEmailSender _EmailSender;
        private readonly RoleManager<IdentityRole> _roleManager;

        public string message { get; set; }

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<AccountController> logger, Cloudinary Cloudinary, AppDbContext Context, IEmailSender EmailSender, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _Cloudinary = Cloudinary;
            _Context = Context;
            _EmailSender = EmailSender;
            _roleManager = roleManager;
        }

        [HttpGet("/login/")]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("/login/")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(ViewLogin model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (!result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null) result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                }
                if (result.Succeeded) return LocalRedirect("~/");

            }
            return View(model);
        }

        [HttpGet("/register/")]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost("/register/")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(ViewRegister model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Email);
                if (user != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được đăng ký!, vui lòng sử dụng email khác.");
                    return View(model);
                }
                var newUser = new User()
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    BirthDate = model.BirthDate,
                    Gender = model.Gender
                };

                var result = await _userManager.CreateAsync(newUser, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Đã tạo user mới.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var callbackUrl = Url.ActionLink(
                        action: nameof(ConfirmEmail),
                        values:
                            new
                            {
                                userId = newUser.Id,
                                code = code
                            },
                        protocol: Request.Scheme);

                    await _EmailSender.SendEmailAsync(model.Email,
                        "Xác nhận địa chỉ email",
                        @$"Bạn đã đăng ký tài khoản trên Myblog, 
                           hãy <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>bấm vào đây</a> 
                           để kích hoạt tài khoản.");

                    await _userManager.AddToRoleAsync(newUser, "User");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return LocalRedirect(Url.Action(nameof(RegisterConfirmation)));
                    }
                    else
                    {
                        await _signInManager.SignInAsync(newUser, isPersistent: false);
                        return LocalRedirect("~/");
                    }
                }
                else
                {
                    foreach (var err in result.Errors)
                    {
                        ModelState.AddModelError("", err.Description);
                    }
                }
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
                return NotFound();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded) return Redirect("~/login");
            else return Redirect("~/");

        }

        [HttpPost("/logout")]
        [Authorize(Roles = "User, Admin, Manage")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return LocalRedirect("~/");
        }

        [HttpGet("/Setting")]
        [Authorize(Roles = "User, Admin, Manage")]
        public async Task<IActionResult> Setting()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");
            var avatar = await _Context.Photos.FirstOrDefaultAsync(p => p.Id == user.AvatarId);
            var userSetting = new ViewSetting()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Gender = user.Gender,
                Avatar = avatar != null ? avatar : new Photo() { Url = "https://vnn-imgs-a1.vgcloud.vn/image1.ictnews.vn/_Files/2020/03/17/trend-avatar-1.jpg" },
                BirthDate = user.BirthDate,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
            };

            return View(userSetting);

        }

        [HttpPost("/Setting/Avatar")]
        [Authorize(Roles = "User, Manage, Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeAvatar(IFormFile avatar)
        {

            if (avatar == null)
            {
                return RedirectToAction("Setting");
            }

            IFormatProvider provider = CultureInfo.CreateSpecificCulture("en-US");
            var image = avatar;
            if (image.Length == 0) return RedirectToAction("Setting");

            var result = await _Cloudinary.UploadAsync(new ImageUploadParams
            {
                File = new FileDescription(image.FileName,
                    image.OpenReadStream()),
            });

            var avatarPhoto = new Photo
            {
                Bytes = (int)result.Bytes,
                CreatedAt = DateTime.Now,
                Format = result.Format,
                Height = result.Height,
                Path = result.Url.AbsolutePath,
                PublicId = result.PublicId,
                ResourceType = result.ResourceType,
                SecureUrl = result.SecureUrl.AbsoluteUri,
                Signature = result.Signature,
                Type = result.JsonObj["type"]?.ToString(),
                Url = result.Url.AbsoluteUri,
                Version = int.Parse(result.Version, provider),
                Width = result.Width
            };

            var user = await _userManager.GetUserAsync(User);
            user.Avatar = avatarPhoto;

            await _userManager.UpdateAsync(user);

            return RedirectToAction("Setting");
        }

        [HttpPost("/Setting")]
        [Authorize(Roles = "User, Manage, Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setting(ViewUserInfo model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            if (ModelState.IsValid)
            {

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Gender = model.Gender;
                user.BirthDate = model.BirthDate;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Setting");
                }
            }

            var avatar = await _Context.Photos.FirstOrDefaultAsync(p => p.Id == user.AvatarId);
            var userSetting = new ViewSetting();
            userSetting.Avatar = avatar;

            var typeOfUserSetting = userSetting.GetType();
            foreach (var prop in model.GetType().GetProperties())
            {
                typeOfUserSetting.GetProperty(prop.Name).SetValue(userSetting, prop.GetValue(model));
            }

            return View(userSetting);
        }

        [HttpGet("/MyItems")]
        [Authorize(Roles = "User, Manage, Admin")]
        public async Task<IActionResult> MyItems()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var posts = await _Context.Posts.Include(p => p.Thumbnail).Where(p => p.AuthorId == user.Id).ToListAsync();
            return View(posts);
        }

        [HttpPost]
        [Authorize(Roles = "User, Manage, Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ViewPassword model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {

                var result = await _userManager.ChangePasswordAsync(user, model.CurentPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    return RedirectToAction("Setting");
                }
                else
                {
                    ModelState.AddModelError("", "Không thành công");
                }
            }

            var avatar = await _Context.Photos.FirstOrDefaultAsync(p => p.Id == user.AvatarId);
            var userSetting = new ViewSetting()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Gender = user.Gender,
                BirthDate = user.BirthDate,
                Email = user.Email,
                Avatar = avatar,
                PhoneNumber = user.PhoneNumber,
                CurentPassword = model.CurentPassword,
                NewPassword = model.NewPassword,
                ConfirmPassword = model.ConfirmPassword
            };
            return View("Setting", userSetting);
        }

    }
}
