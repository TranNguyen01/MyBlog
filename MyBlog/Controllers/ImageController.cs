using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyBlog.Models;
using System;
using System.Collections.Generic;

namespace MyBlog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly Cloudinary _Cloudinary;
        private readonly AppDbContext _Context;
        private readonly ILogger<ImageController> _Logger;
        private readonly UserManager<User> _UserManager;

        public ImageController(Cloudinary cloudinary, AppDbContext context, ILogger<ImageController> logger, UserManager<User> userManager)
        {
            _Cloudinary = cloudinary;
            _Context = context;
            _Logger = logger;
            _UserManager = userManager;
        }

        [HttpGet("/image/signature")]
        public IActionResult GetCloudinarySignInfo()
        {
            var now = (DateTimeOffset)DateTime.Now;
            var timestamp = now.ToUnixTimeMilliseconds();

            var signature = _Cloudinary.Api.SignParameters(
                new Dictionary<string, object>
                {
                    {"folder", "Blog" },
                    { "timestamp",  timestamp},
                }
            );

            return Ok(new
            {
                cloudname = _Cloudinary.Api.Account.Cloud,
                signature = signature,
                api_key = _Cloudinary.Api.Account.ApiKey,
                timestamp = timestamp,
                url = $"https://api.cloudinary.com/v1_1/{_Cloudinary.Api.Account.Cloud}/image/upload",
                folder = "Blog"
            });
        }
    }
}
