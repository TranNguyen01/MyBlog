using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyBlog.Services;
using MyBlog.Models;
using MyBlog.Models.ViewModels;
using MyBlog.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MyBlog.Service;
using System.ComponentModel;

namespace MyBlog.Controllers
{
    [Route("/Post")]
    public class PostController : Controller
    {
        private const string FolderName = "preset_folder";
        private readonly AppDbContext _Context;
        private readonly ILogger<PostController> _Logger;
        private readonly UserManager<User> _UserManager;
        private readonly Cloudinary _cloudinary;
        private readonly IResponseCacheService _ResponseCacheService;
        private readonly IElasticsearch _ESClient;
        public string message { get; set; }

        public PostController(AppDbContext context, ILogger<PostController> logger, UserManager<User> UserManager, Cloudinary Cloudinary, IResponseCacheService responseCacheService, IElasticsearch eSClient)
        {
            _Context = context;
            _Logger = logger;
            _UserManager = UserManager;
            _cloudinary = Cloudinary;
            _ResponseCacheService = responseCacheService;
            _ESClient = eSClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 15)
        {
            string cacheKey = _ResponseCacheService.generateResponseCacheKey(
                "/post/index",
                new List<(string, string)> {
                    ("page", page.ToString()),
                    ("pageSize", pageSize.ToString())
                }
            );

            string cacheData = await _ResponseCacheService.GetResponseCacheAsync(cacheKey);
            List<Post> posts;
            if (string.IsNullOrEmpty(cacheData))
            {
                posts = await _Context.Posts
                    .Include(c => c.Author)
                    .Include(c => c.Category)
                    .Include(c => c.Thumbnail)
                    .Where(p => p.Deleted == false)
                    .OrderBy(p=>p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            else
            {
                posts = JsonConvert.DeserializeObject<List<Post>>(cacheData);
            }

            return View(posts);
        }

        [HttpGet("manage")]
        [Authorize]
        public async Task<IActionResult> Manage(int page = 1, int pageSize = 15)
        {


            string cacheKey = _ResponseCacheService.generateResponseCacheKey(
               "/post/manage",
               new List<(string, string)> {
                    ("page", page.ToString()),
                    ("pageSize", pageSize.ToString())
               }
           );

            string cacheData = await _ResponseCacheService.GetResponseCacheAsync(cacheKey);
            List<Post> posts;
            if (string.IsNullOrEmpty(cacheData))
            {
                posts = await _Context.Posts
                .Include(c => c.Author)
                .Include(c => c.Category)
                .OrderBy(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Where(p => p.Deleted == false)
                .ToListAsync();

                await _ResponseCacheService.SetResponseCacheAsync(cacheKey, posts, TimeSpan.FromMinutes(1));
            }
            else
            {
                posts = JsonConvert.DeserializeObject<List<Post>>(cacheData);
            }

            return View(posts);
        }

        [HttpGet("Create")]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            string Preset = $"sample_{_cloudinary.Api.SignParameters(new SortedDictionary<string, object> { { "api_key", _cloudinary.Api.Account.ApiKey } }).Substring(0, 10)}";

            await _cloudinary.CreateUploadPresetAsync(new UploadPresetParams
            {
                Name = Preset,
                Unsigned = true,
                Folder = FolderName
            }).ConfigureAwait(false);
            ViewData["AllCategories"] = new SelectList(_Context.Categories.Where(c => c.Deleted == false).ToList(), "Id", "Name");
            return View("~/Views/Account/CreatePost.cshtml");
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search(string query, [FromQuery][DefaultValue(1)] int page, [FromQuery][DefaultValue(15)] int pageSize, [FromQuery] [DefaultValue("bai-viet")] string type )
        {
            if (query == null) return Redirect("~");

            List<Post> posts;
            List<Document> documents;
            Category searchResult;

            if (true)
            {
                
                
                //return Ok(searchResult1);
               

                if(type == "bai-viet")
                {
                    var searchResult1 = await _ESClient.Search<Post>("post", query, page, pageSize);
                    var postIds = searchResult1.Data.Select(p => p.Id).ToList();
                    posts = await _Context.Posts
                       .Include(p => p.Author)
                       .Include(p => p.Category)
                       .Include(p => p.Thumbnail)
                       .Where(p => p.Deleted == false)
                       .Where(p => postIds.Contains(p.Id))
                       .Where(p=>p.Status == 1)
                       .OrderBy(p => p.CreatedAt)
                       .Skip((page - 1) * pageSize)
                       .Take(pageSize)
                       .ToListAsync();
                    searchResult = new Category()
                    {
                        Name = $"Kết quả tìm kiếm cho \"{query}\"",
                        ParentCategory = null,
                        ChildrenCategory = null,
                        Posts = posts,
                        Documents = null,
                        Description = "",
                        Slug = $"search?query={query}"
                    };
                }
                else
                {
                    var searchResultDocument = await _ESClient.Search<Document>("document1", query, page, pageSize);
                    var documentIds = searchResultDocument.Data.Select(d => d.Id).ToList();
                    documents = await _Context.Documents
                         .Include(p => p.Author)
                         .Include(p => p.Category)
                         .Where(p => p.Deleted == false)
                         .Where(p => documentIds.Contains(p.Id))
                         .OrderBy(p => p.Dowloaded)
                         .Skip((page - 1) * pageSize)
                         .Take(pageSize)
                         .ToListAsync();
                    searchResult = new Category()
                    {
                        Name = $"Kết quả tìm kiếm cho \"{query}\"",
                        ParentCategory = null,
                        ChildrenCategory = null,
                        Posts = null,
                        Documents = documents,
                        Description = "",
                        Slug = $"search?query={query}"
                    };
                }
            }
            else
            {
                //searchResult = JsonConvert.DeserializeObject<Category>(cacheData);
                //if (type == "bai-viet")
                //    searchResult.Documents = null;
                //else
                //    searchResult.Posts = null;
            }
            return View("~/Views/Categories/Details.cshtml", searchResult);
        }

        [HttpGet("{id}/edit")]
        [Authorize]
        public async Task<IActionResult> Edit(string id)
        {
            var post = await _Context.Posts
                .Include(c => c.Author)
                .Include(c => c.Category)
                .Include(c => c.Thumbnail)
                .FirstOrDefaultAsync(c => c.Id == Guid.Parse(id));

            if (post == null) return NotFound();
            ViewData["AllCategories"] = new SelectList(_Context.Categories, "Id", "Name");

            var viewPost = new ViewPost()
            {
                Id = post.Id.ToString(),
                Title = post.Title,
                Category = post.Category,
                CategoryId = post.CategoryId.ToString(),
                Description = post.Description,
                Content = post.Content
            };
            ViewData["Thumbnail"] = post.Thumbnail;
            ViewData["AllCategories"] = new SelectList(_Context.Categories, "Id", "Name");
            return View("~/Views/Account/EditPost.cshtml", viewPost);
        }

        [HttpPost("{id}/edit")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string? id, [Bind("Id,Title,Description,Content,AuthorId,CategoryId,Thumbnail")] ViewPost post)
        {
            post.Id = id;
            var updatePost = await _Context.Posts
                .Include(c => c.Thumbnail)
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == Guid.Parse(id));

            string cacheKey1 = _ResponseCacheService.generateResponseCacheKey($"/post/{updatePost.Slug}", new List<(string, string)>());
            await _ResponseCacheService.ClearResponseCacheAsync(cacheKey1);

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

                updatePost.CategoryId = Guid.Parse(post.CategoryId);
                updatePost.Title = post.Title;
                updatePost.Description = post.Description == null ? string.Empty : post.Description;
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
            string cacheKey = _ResponseCacheService.generateResponseCacheKey($"/post/{updatePost.Slug}", new List<(string, string)>());
            string cacheIndexkey = _ResponseCacheService.generateResponseCacheKey($"/post/index", new List<(string, string)>());
            await _ResponseCacheService.ClearResponseCacheAsync(cacheKey);
            await _ResponseCacheService.ClearResponseCacheAsync(cacheIndexkey);

            ViewData["AllCategories"] = new SelectList(_Context.Categories, "Id", "Name");
            ViewData["Thumbnail"] = thumbnail;
            return View("~/Views/Account/EditPost.cshtml", post);

        }

        [HttpPost]
        [Route("Create")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Content,CategoryId,Thumbnail")] ViewPost post)
        {
            var user = await _UserManager.GetUserAsync(HttpContext.User);
            if (user == null) Redirect("/Login");

            Photo thumbnail = new Photo();
            //Parse image upload result from cloudinary
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
                    //Generate slug
                    var slug = AppUtilities.GenerateSlug(post.Title);
                    for (int index = 1; await _Context.Posts.AnyAsync(p => p.Slug == slug); index++)
                    {
                        slug = AppUtilities.GenerateSlug(post.Title) + "-" + index;
                    }

                    //Create model
                    var newPost = new Post()
                    {
                        Title = post.Title,
                        Slug = slug,
                        CategoryId = Guid.Parse(post.CategoryId),
                        AuthorId = user.Id,
                        Description = post.Description == null?string.Empty: post.Description,
                        Content = post.Content,
                        CreatedAt = DateTime.Now,
                        LastUpdatedAt = DateTime.Now,
                        Thumbnail = thumbnail,
                        Status = 0,
                    };

                    //Save data to DB
                    _Context.Photos.Add(thumbnail);
                    _Context.Posts.Add(newPost);
                    await _Context.SaveChangesAsync();

                    //Index to elasticsearch
                    if (!_ESClient.CheckExistIndex("post"))
                    {
                        await _ESClient.CreatePostIndexAsync();
                    }
                    _ = await _ESClient.Index(newPost, "post", newPost.Id.ToString());
                    return Redirect("/Post/" + newPost.Slug);
                }
                catch (Exception ex)
                {
                    _Logger.LogError(ex.Message);
                    ModelState.AddModelError(String.Empty, "Tạo mới không thành công");
                }
            }

            string cacheKey = _ResponseCacheService.generateResponseCacheKey("/post/index", new List<(string, string)>());
            await _ResponseCacheService.ClearResponseCacheAsync(cacheKey);
            ViewData["Thumbnail"] = thumbnail;
            ViewData["AllCategories"] = new SelectList(_Context.Categories, "Id", "Name");
            return View("~/Views/Account/CreatePost.cshtml", post);

        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var post = await _Context.Posts.FirstOrDefaultAsync(c => c.Id == Guid.Parse(id));
            if (post == null) return NotFound();

            try
            {
                post.Deleted = true;
                _Context.Entry(post).State = EntityState.Modified;
                await _Context.SaveChangesAsync();
                message = "Xoá thành công!";
                string cacheKey = _ResponseCacheService.generateResponseCacheKey($"/post/{post.Slug}", new List<(string, string)>());
                string cacheIndexkey = _ResponseCacheService.generateResponseCacheKey($"/post/index", new List<(string, string)>());
                await _ResponseCacheService.ClearResponseCacheAsync(cacheKey);
                await _ResponseCacheService.ClearResponseCacheAsync(cacheIndexkey);
                return Redirect("/MyItems");
            }
            catch (Exception ex)
            {
                message = "Xoá không thành công!";
                return Redirect("/MyItems");
            }
        }

        [HttpGet("content/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetContent([FromRoute] string id)
        {
            string cacheKey = _ResponseCacheService.generateResponseCacheKey($"/post/content/{id}", new List<(string, string)>());
            string cacheData = await _ResponseCacheService.GetResponseCacheAsync(cacheKey);
            Post post;
            if (string.IsNullOrEmpty(cacheData))
            {
                post = await _Context.Posts
                    .Include(c => c.Author)
                    .Include(c => c.Category)
                    .Include(c => c.Thumbnail)
                    .Include(c => c.Comments)
                    .ThenInclude(cm => cm.User)
                    .ThenInclude(u => u.Avatar)
                    .Include(c => c.Likes)
                    .Where(c => c.Deleted == false)
                    .FirstOrDefaultAsync(c => c.Id  == Guid.Parse(id));
                if (post == null)
                    return NotFound();
                await _ResponseCacheService.SetResponseCacheAsync(cacheKey, post, TimeSpan.FromMinutes(1));
            }
            else
            {
                post = JsonConvert.DeserializeObject<Post>(cacheData);
            }

            if (post == null) return NotFound();
            else return PartialView("_PostDetail", post);
        }

        [HttpGet("{slug}")]
        [AllowAnonymous]
        public async Task<IActionResult> Details(string slug)
        {
            string cacheKey = _ResponseCacheService.generateResponseCacheKey($"/post/{slug}", new List<(string, string)>());
            string cacheData = await _ResponseCacheService.GetResponseCacheAsync(cacheKey);
            Post post;
            if (string.IsNullOrEmpty(cacheData))
            {



                post = await _Context.Posts
                    .Include(c => c.Author)
                    .Include(c => c.Category)
                    .Include(c => c.Thumbnail)
                    .Where(c => c.Deleted == false)
                    .FirstOrDefaultAsync(c => c.Slug == slug);
                if (post == null)
                    return NotFound();
                await _ResponseCacheService.SetResponseCacheAsync(cacheKey, post, TimeSpan.FromMinutes(1));
            }
            else
            {
                post = JsonConvert.DeserializeObject<Post>(cacheData);
            }

            if (post == null) return NotFound();
            else return View(post);
        }

        public async Task<bool> IsValidSlug(Post post, string slug)
        {
            return !(await _Context.Posts.AnyAsync(p => p.Slug == slug && p.Id != post.Id));
        }

    }
}
