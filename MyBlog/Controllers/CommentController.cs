using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyBlog.Models;
using MyBlog.Models.ViewModels;
using System;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    public class CommentController : Controller
    {

        private readonly AppDbContext _Context;
        private readonly ILogger<CommentController> _Logger;
        private readonly UserManager<User> _UserManager;

        public string message { get; set; }

        public CommentController(AppDbContext context, ILogger<CommentController> logger, UserManager<User> userManager)
        {
            _Context = context;
            _Logger = logger;
            _UserManager = userManager;
        }

        [HttpPost]
        [Authorize(Roles = "User, Manage, Admin")]
        public async Task<IActionResult> Create([Bind("PostId,Content")] ViewComment comment)
        {
            var user = await _UserManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {
                var post = await _Context.Posts.FirstOrDefaultAsync(p => p.Id == comment.PostId);
                if (post == null) return NotFound();
                var newComment = new Comment()
                {
                    UserId = user.Id,
                    PostId = post.Id,
                    Content = comment.Content,
                    CreatedAt = DateTime.Now
                };

                _Context.Comments.Add(newComment);

                try
                {
                    await _Context.SaveChangesAsync();
                    return Json(new
                    {
                        success = true,
                        message = "Thêm bình luận mới thành công",
                        comment = comment
                    });
                }
                catch (Exception ex)
                {
                    _Logger.LogError(ex.Message);
                }
            }

            return Json(new
            {
                success = false,
                message = "Bình luận không thành công!"
            });
        }

        [Authorize(Roles = "User, Manage, Admin")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int Id)
        {
            var comment = await _Context.Comments.FirstOrDefaultAsync(cmt => cmt.Id == Id);
            if (comment == null) return Json(new { success = false, code = NotFound().StatusCode, message = "Bình luận không tồn tại!" });

            var user = await _UserManager.GetUserAsync(User);

            if (user == null)
                return Json(new { success = false, code = Unauthorized().StatusCode, message = "Đăng nhập để xoá!" });

            if (user.Id != comment.UserId)
                return Json(new { success = false, code = Unauthorized().StatusCode, message = "Đăng nhập để xoá!" });

            try
            {
                _Context.Comments.Remove(comment);
                await _Context.SaveChangesAsync();
                return Json(new { success = true, code = 200, message = "Xoá thành công" });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Json(new { success = false, code = 500, message = "Xoá không thành công" });
            }
        }
    }
}
