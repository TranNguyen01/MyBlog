using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyBlog.Models;
using MyBlog.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [HttpGet]
        [Route("/Like")]
        public async Task<BaseResponse<List<Like>>> GetLikesByPostId([FromQuery] string postId)
        {
            var likes = await _Context.Likes.Where(l => l.PostId == Guid.Parse(postId)).ToListAsync();
            return new BaseResponse<List<Like>>
            {
                Code = 0,
                message = "",
                Data = likes
            };
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Like([Bind("UserId, PostId")] Like like)
        {
            if (!ModelState.IsValid) return Json(new { success = false, message = "Không thành công" });

            var existLike = await _Context.Likes.FirstOrDefaultAsync(l => l.PostId == like.PostId && l.UserId == like.UserId);
            try
            {
                if (existLike != null)
                {
                    _Context.Likes.Remove(existLike);
                    Post post = await _Context.Posts.FirstOrDefaultAsync(p => p.Id == existLike.PostId);
                    if (post != null && post.LikesCount > 1) post.LikesCount--;
                }
                else
                {
                    Post post = await _Context.Posts.FirstOrDefaultAsync(p => p.Id == like.PostId);
                    if (post != null) post.LikesCount++;
                    else return Json(new BaseResponse<Like> { Code = 0, message = "Error", Data = null });
                    _Context.Likes.Add(like);
                }
                await _Context.SaveChangesAsync();
                return Json(new { success = true, message = "Thành công!", like = like });
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex.Message);
                return Json(new { success = false, message = "Không thành công!" });
            }
        }
    }
}
