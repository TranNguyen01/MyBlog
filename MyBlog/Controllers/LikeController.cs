using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyBlog.Models;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    public class LikeController : Controller
    {

        private readonly AppDbContext _Context;
        private readonly ILogger<LikeController> _Logger;
        private readonly UserManager<User> _UserManager;

        public LikeController(AppDbContext context, ILogger<LikeController> logger, UserManager<User> userManager)
        {
            _Context = context;
            _Logger = logger;
            _UserManager = userManager;
        }

        public async Task<IActionResult> Create([Bind("UserId,PostId")] Like like)
        {
            if (!ModelState.IsValid) return Json(new { success = false, message = "Không thành công" });
            _Context.Likes.Add(like);
            try
            {
                await _Context.SaveChangesAsync();
                return Json(new { success = true, message = "Thêm thành công!", like = like });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Json(new { success = false, message = "Không thành công!" });
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var like = await _Context.Likes.FirstOrDefaultAsync(l => l.Id == id);
            if (like == null) return Json(new { succes = true, message = "Không tìm thấy!" });
            _Context.Likes.Remove(like);

            try
            {
                await _Context.SaveChangesAsync();
                return Json(new { success = true, message = "Thêm thành công!" });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Json(new { success = true, message = "Không thành công!" });
            }
        }
    }
}
