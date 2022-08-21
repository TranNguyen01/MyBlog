using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyBlog.Models;
using MyBlog.Models.ViewModels;
using MyBlog.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    public class PostController : Controller
    {
        private const string FolderName = "preset_folder";
        private readonly AppDbContext _Context;
        private readonly ILogger<PostController> _Logger;
        private readonly UserManager<User> _UserManager;
        private readonly Cloudinary _cloudinary;
        public string message { get; set; }

        public PostController(AppDbContext context, ILogger<PostController> logger, UserManager<User> UserManager, Cloudinary Cloudinary)
        {
            _Context = context;
            _Logger = logger;
            _UserManager = UserManager;
            _cloudinary = Cloudinary;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 15)
        {
            var post = await _Context.Posts
                .Include(c => c.Author)
                .Include(c => c.Category)
                .Include(c => c.Thumbnail)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return View(post);
        }

        [HttpGet("/post/manage")]
        public async Task<IActionResult> Manage(int page = 1, int pageSize = 15)
        {
            var post = await _Context.Posts
                .Include(c => c.Author)
                .Include(c => c.Category)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return View(post);
        }

        [HttpGet("/post/create")]
        public async Task<IActionResult> Create()
        {
            string Preset = $"sample_{_cloudinary.Api.SignParameters(new SortedDictionary<string, object> { { "api_key", _cloudinary.Api.Account.ApiKey } }).Substring(0, 10)}";

            await _cloudinary.CreateUploadPresetAsync(new UploadPresetParams
            {
                Name = Preset,
                Unsigned = true,
                Folder = FolderName
            }).ConfigureAwait(false);
            ViewData["AllCategories"] = new SelectList(_Context.Categories, "Id", "Name");
            return View("~/Views/Account/CreatePost.cshtml");
        }

        [HttpGet("/post/search")]
        public async Task<IActionResult> Search(string query)
        {
            if (query == null) return Redirect("~");

            var posts = await _Context.Posts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.Thumbnail)
                .Where(p => p.Title.ToLower().Contains(query.ToLower()))
                .ToListAsync();

            var searchResult = new Category()
            {
                Name = $"Kết quả tìm kiếm cho \"{query}\"",
                ParentCategory = null,
                ChildrenCategory = null,
                Posts = posts,
                Description = "",
                Slug = ""
            };

            return View("~/Views/Categories/Details.cshtml", searchResult);
        }

        [HttpGet("/post/{id}/edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _Context.Posts
                .Include(c => c.Author)
                .Include(c => c.Category)
                .Include(c => c.Thumbnail)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (post == null) return NotFound();
            ViewData["AllCategories"] = new SelectList(_Context.Categories, "Id", "Name");

            var viewPost = new ViewPost()
            {
                Id = post.Id,
                Title = post.Title,
                Category = post.Category,
                CategoryId = (int)post.CategoryId,
                Description = post.Description,
                Content = post.Content
            };
            ViewData["Thumbnail"] = post.Thumbnail;
            ViewData["AllCategories"] = new SelectList(_Context.Categories, "Id", "Name");
            return View("~/Views/Account/EditPost.cshtml", viewPost);
        }

        [HttpPost("post/{id}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,Title,Description,Content,AuthorId,CategoryId,Thumbnail")] ViewPost post)
        {
            if (id != post.Id) return NotFound();

            var updatePost = await _Context.Posts
                .Include(c => c.Thumbnail)
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (updatePost == null) return NotFound();

            Photo thumbnail = updatePost.Thumbnail;
            if (!string.IsNullOrEmpty(post.Thumbnail))
            {
                try
                {
                    var parsedResult = JsonConvert.DeserializeObject<ImageUploadResult>(post.Thumbnail);
                    IFormatProvider provider = CultureInfo.CreateSpecificCulture("en-US");
                    thumbnail = new Photo
                    {
                        CreatedAt = parsedResult.CreatedAt,
                        Format = parsedResult.Format,
                        Height = parsedResult.Height,
                        PublicId = parsedResult.PublicId,
                        ResourceType = parsedResult.ResourceType,
                        SecureUrl = parsedResult.SecureUrl.ToString(),
                        Signature = parsedResult.Signature,
                        Type = parsedResult.Type,
                        Url = parsedResult.Url.ToString(),
                        Version = int.Parse(parsedResult.Version, provider),
                        Width = parsedResult.Width,
                    };
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(String.Empty, "Ảnh bìa không hợp lệ");
                }
            }



            if (ModelState.IsValid)
            {
                if (updatePost.Title.ToLower() != post.Title.ToLower())
                {
                    var newSlug = AppUtilities.GenerateSlug(post.Title);
                    for (int index = 1; !(await IsValidSlug(updatePost, newSlug)); index++)
                    {
                        newSlug = AppUtilities.GenerateSlug(post.Title) + "-" + index;
                    }
                    updatePost.Slug = newSlug;
                }

                if (!string.IsNullOrEmpty(post.Thumbnail))
                {
                    var photo = _Context.Photos.FirstOrDefault(p => p.Id == updatePost.Thumbnail.Id);
                    _Context.Photos.Remove(updatePost.Thumbnail);
                }

                updatePost.CategoryId = (int)post.CategoryId;
                updatePost.Title = post.Title;
                updatePost.Description = post.Description;
                updatePost.Content = post.Content;
                updatePost.LastUpdatedAt = DateTime.Now;
                updatePost.Thumbnail = thumbnail;

                try
                {
                    await _Context.SaveChangesAsync();
                    return RedirectToAction(updatePost.Slug, "Post");
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "Không thành công");
                }
            }

            ViewData["AllCategories"] = new SelectList(_Context.Categories, "Id", "Name");
            ViewData["Thumbnail"] = thumbnail;
            return View("~/Views/Account/EditPost.cshtml", post);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Content,CategoryId,Thumbnail")] ViewPost post)
        {

            var user = await _UserManager.GetUserAsync(HttpContext.User);
            if (user == null) ModelState.AddModelError("", "Phải đăng nhập để tạo bài viết mới");

            Photo thumbnail = new Photo();
            if (string.IsNullOrEmpty(post.Thumbnail)) ModelState.AddModelError("Thumbnail", "Ảnh bìa là bắt buộc!");
            else
            {
                try
                {
                    var parsedResult = JsonConvert.DeserializeObject<ImageUploadResult>(post.Thumbnail);
                    IFormatProvider provider = CultureInfo.CreateSpecificCulture("en-US");
                    thumbnail.CreatedAt = parsedResult.CreatedAt;
                    thumbnail.Format = parsedResult.Format;
                    thumbnail.Height = parsedResult.Height;
                    thumbnail.PublicId = parsedResult.PublicId;
                    thumbnail.ResourceType = parsedResult.ResourceType;
                    thumbnail.SecureUrl = parsedResult.SecureUrl.ToString();
                    thumbnail.Signature = parsedResult.Signature;
                    thumbnail.Type = parsedResult.Type;
                    thumbnail.Url = parsedResult.Url.ToString();
                    thumbnail.Version = int.Parse(parsedResult.Version, provider);
                    thumbnail.Width = parsedResult.Width;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(String.Empty, "Ảnh bìa không hợp lệ!");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var slug = AppUtilities.GenerateSlug(post.Title);
                    for (int index = 1; await _Context.Posts.AnyAsync(p => p.Slug == slug); index++)
                    {
                        slug = AppUtilities.GenerateSlug(post.Title) + "-" + index;
                    }

                    await _Context.Photos.AddAsync(thumbnail);

                    var newPost = new Post()
                    {
                        Title = post.Title,
                        Slug = slug,
                        CategoryId = (int)post.CategoryId,
                        AuthorId = user.Id,
                        Description = post.Description,
                        Content = post.Content,
                        CreatedAt = DateTime.Now,
                        LastUpdatedAt = DateTime.Now,
                        Thumbnail = thumbnail
                    };

                    await _Context.Posts.AddAsync(newPost);
                    await _Context.SaveChangesAsync();
                    return RedirectToAction(newPost.Slug, "Post");
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError(String.Empty, "Tạo mới không thành công");
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(String.Empty, "Tạo mới không thành công");
                }
                catch (Exception ex)
                {
                    _Logger.LogError(ex.Message);
                    ModelState.AddModelError(String.Empty, "Tạo mới không thành công");
                }
            }

            ViewData["Thumbnail"] = thumbnail;
            ViewData["AllCategories"] = new SelectList(_Context.Categories, "Id", "Name");
            return View("~/Views/Account/CreatePost.cshtml", post);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _Context.Posts.FirstOrDefaultAsync(c => c.Id == id);
            if (post == null) return NotFound();
            _Context.Posts.Remove(post);

            try
            {
                await _Context.SaveChangesAsync();
                message = "Xoá thành công!";
                return RedirectToAction(nameof(Manage));
            }
            catch (DbUpdateConcurrencyException)
            {
                message = "Xoá không thành công!";
                return RedirectToAction(nameof(Manage));
            }
            catch (DbUpdateException)
            {
                message = "Xoá không thành công!";
                return RedirectToAction(nameof(Manage));
            }
        }

        [HttpGet("/post/{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            var post = await _Context.Posts
                .Include(c => c.Author)
                .Include(c => c.Category)
                .Include(c => c.Thumbnail)
                .Include(c => c.Comments)
                .ThenInclude(cm => cm.User)
                .ThenInclude(u => u.Avatar)
                .FirstOrDefaultAsync(c => c.Slug == slug);

            if (post == null) return NotFound();
            else return View(post);
        }

        public async Task<bool> IsValidSlug(Post post, string slug)
        {
            return !(await _Context.Posts.AnyAsync(p => p.Slug == slug && p.Id != post.Id));
        }

    }
}
