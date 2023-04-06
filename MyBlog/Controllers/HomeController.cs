using MailKit.Search;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyBlog.Models;
using MyBlog.Models.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _Context;
        private readonly UserManager<User> _UserManager;
        private readonly RoleManager<IdentityRole> _RoleManager;
        public string message { get; set; }
        public HomeController(ILogger<HomeController> logger, AppDbContext context, UserManager<User> UserManager, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _Context = context;
            _UserManager = UserManager;
            _RoleManager = roleManager;
        }

        public async Task<IActionResult> IndexAsync(int pageSize = 9, int page = 1)
        {
            var user = await _UserManager.GetUserAsync(User);
            if(user != null)
            {
                var role = await _UserManager.GetRolesAsync(user);
                if(role.Contains("Admin") || role.Contains("Manage") || role.Contains("Censor"))
                {
                    return Redirect("Statistic");
                }
            }

            //var post = await _Context.Posts
            //  .Include(c => c.Author)
            //  .Include(c => c.Category)
            //  .Include(c => c.Thumbnail)
            //  .Where(c => c.Deleted == false && c.Status == 1)
            //  .OrderBy(c => c.CreatedAt)
            //  .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var documents = await _Context.Documents
                .Include(d => d.Author)
                .Include(d => d.Category)
                .Where(c => c.Deleted == false)
                .OrderBy(c => c.CreatedAt)
                .Skip(0).Take(6)
                .ToListAsync();

            var categories = await _Context.Categories
               .Include(c => c.ParentCategory)
               .Include(c => c.ChildrenCategory)
               .Where(c => c.ParentCategoryId == null && c.Deleted == false)
               .ToListAsync();

            var likePost = from l in _Context.Likes
                               group l by l.PostId into glp
                               select new
                               {
                                   Id = glp.Key,
                                   Likes = glp.Count()
                               };

            var hostPostId = await likePost.OrderBy(c => c.Likes).Select(c=>c.Id).ToListAsync();
            var hostPosts = await _Context.Posts
                .Include(c => c.Author)
                .ThenInclude(a=>a.Avatar)
                .Include(c => c.Category)
                .Include(c => c.Thumbnail)
                .Where(p => hostPostId.Contains(p.Id))
                .Where(c => c.Deleted == false && c.Status == 1)
                .Skip(0).Take(10).ToListAsync();

            if(hostPostId.Count < 10)
            {
                var addDocuments = await _Context.Posts
                  .Include(c => c.Author)
                  .ThenInclude(a => a.Avatar)
                  .Include(c => c.Category)
                  .Include(c => c.Thumbnail)
                  .Where(c => c.Deleted == false && c.Status == 1 && !hostPostId.Contains(c.Id))
                  .OrderBy(c => c.CreatedAt)
                  .Take(10 - hostPostId.Count).ToListAsync();
                hostPosts.AddRange(addDocuments);
            }

            foreach (var category in categories)
            {
                await GetChildrenCategory(category);
            }

            int countPosts = _Context.Posts.Count(c => c.Deleted == false);

            ViewData["Categories"] = categories;
            ViewData["maxPage"] = (int)countPosts / pageSize + (countPosts % pageSize == 0 ? 0 : 1);
            ViewData["currentPage"] = page;
            return View("~/Views/Home/HomePage.cshtml",
                new ViewHome
                {
                    Categories = categories,
                    Documents = documents,
                    Feature = null,
                    Posts = hostPosts
                }
                );
        }

        [HttpGet]
        public async Task<IActionResult> Post(
            [FromQuery(Name = "searchKey")] string searchKey,
            [FromQuery(Name = "page")] int page = 1,
            [FromQuery(Name = "pageSize")] int pageSize = 100)
        {
            var post = await _Context.Posts
             .Include(c => c.Author)
             .Include(c => c.Category)
             .Include(c => c.Thumbnail)
             .Where(c => c.Deleted == false)
             .OrderBy(c => c.CreatedAt)
             .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var categories = await _Context.Categories
               .Include(c => c.ParentCategory)
               .Include(c => c.ChildrenCategory)
               .Where(c => c.ParentCategoryId == null && c.Deleted == false)
               .ToListAsync();

            foreach (var category in categories)
            {
                await GetChildrenCategory(category);
            }

            int countPosts = _Context.Posts.Count(c => c.Deleted == false);

            return View("~/Views/Home/HomePost.cshtml",
                new ViewHome
                {
                    Categories = categories,
                    Documents = null,
                    Feature = null,
                    Posts = post,
                    CurrentPage = page,
                    MaxPage = countPosts % pageSize == 0 ? countPosts / pageSize : countPosts / pageSize + 1,
                    PageSize = pageSize
                }
                );
        }

        [HttpGet]
        public async Task<IActionResult> Document(
           [FromQuery(Name = "searchKey")] string searchKey,
           [FromQuery(Name = "page")] int page = 1,
           [FromQuery(Name = "pageSize")] int pageSize = 10)
        {
            var documents = await _Context.Documents
                  .Include(d => d.Author)
                  .Include(d => d.Category)
                  .Where(c => c.Deleted == false)
                  .OrderBy(c => c.CreatedAt)
                  .Skip((page - 1) * pageSize).Take(pageSize)
                  .ToListAsync();

            var categories = await _Context.Categories
               .Include(c => c.ParentCategory)
               .Include(c => c.ChildrenCategory)
               .Where(c => c.ParentCategoryId == null && c.Deleted == false)
               .ToListAsync();

            foreach (var category in categories)
            {
                await GetChildrenCategory(category);
            }

            int countDocument = _Context.Documents.Count(c => c.Deleted == false);

            return View("~/Views/Home/HomeDocument.cshtml",
                new ViewHome
                {
                    Categories = categories,
                    Documents = documents,
                    Feature = null,
                    Posts = null,
                    CurrentPage = page,
                    MaxPage = countDocument % pageSize == 0 ? countDocument / pageSize : countDocument / pageSize + 1,
                    PageSize = pageSize
                }
                );
        }

        public async Task GetChildrenCategory(Category ParentCategoies)
        {
            if (ParentCategoies.ChildrenCategory != null && ParentCategoies.ChildrenCategory.Count() > 0)
            {
                ICollection<Category> newChildrenCategory = new List<Category>();
                foreach (var category in ParentCategoies.ChildrenCategory)
                {
                    var newChildCategory = await _Context.Categories
                        .Include(c => c.ParentCategory)
                        .Include(c => c.ChildrenCategory)
                        .FirstOrDefaultAsync(c => c.Id == category.Id && c.Deleted == false);

                    await GetChildrenCategory(newChildCategory);
                    newChildrenCategory.Add(newChildCategory);
                }
                ParentCategoies.ChildrenCategory = newChildrenCategory;
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
