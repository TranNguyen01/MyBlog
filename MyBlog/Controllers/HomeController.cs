using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyBlog.Models;
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
        public string message { get; set; }
        public HomeController(ILogger<HomeController> logger, AppDbContext context, UserManager<User> UserManager)
        {
            _logger = logger;
            _Context = context;
            _UserManager = UserManager;
        }

        public async Task<IActionResult> IndexAsync(int pageSize = 15, int page = 1)
        {
            var post = await _Context.Posts
              .Include(c => c.Author)
              .Include(c => c.Category)
              .Include(c => c.Thumbnail)
              .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var categories = await _Context.Categories
               .Include(c => c.ParentCategory)
               .Include(c => c.ChildrenCategory)
               .Where(c => c.ParentCategoryId == null)
               .ToListAsync();
            foreach (var category in categories)
            {
                await GetChildrenCategory(category);
            }
            ViewData["Categories"] = categories;
            return View("~/Views/Post/Index.cshtml", post);
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
                        .FirstOrDefaultAsync(c => c.Id == category.Id);

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
