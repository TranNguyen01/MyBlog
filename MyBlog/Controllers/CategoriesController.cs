using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    [Route("/Categories")]
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(AppDbContext context, ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {

            var categories = await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.ChildrenCategory)
                .Where(c => c.ParentCategory == null && c.Deleted == false)
                .ToListAsync();

            foreach (var category in categories)
            {
                await GetChildrenCategory(category);
            }
            return View("manage", categories);
        }

        [HttpGet]
        [Route("GetList")]
        public async Task<IActionResult> GetList()
        {
            var categories = await _context.Categories
               .Where(c => c.Deleted == false)
               .ToListAsync();
            return Ok(categories);
        }

        [HttpGet]
        [Route("Manage/")]
        [Authorize(Roles = "Manage,Admin")]
        public async Task<IActionResult> Manage(int page = 1, int pageSize = 15)
        {
            var categories = await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.ChildrenCategory)
                .Where(c => c.ParentCategoryId == null && c.Deleted == false)
                .Skip((page - 1) * pageSize)
                .Take(pageSize).ToListAsync();
            foreach (var category in categories)
            {
                await GetChildrenCategory(category);
            }
            return View(categories);
        }

        public void showCategory(ICollection<Category> categories)
        {
            foreach (var category in categories)
            {
                _logger.LogDebug(category.Name);
                if (category.ChildrenCategory != null && category.ChildrenCategory.Count > 0)
                    showCategory(category.ChildrenCategory);
            }

        }


        public ICollection<Guid> GetCategoriesId(Category category)
        {
            List<Guid> Ids = new List<Guid>();
            Ids.Add((Guid)category.Id);
            if (category.ChildrenCategory != null && category.ChildrenCategory.Count > 0)
            {
                foreach (var cate in category.ChildrenCategory)
                {
                    Ids.Add((Guid)cate.Id);
                    var childrenIds = GetCategoriesId(cate);
                    Ids.AddRange(childrenIds);
                }
            }
            return Ids;
        }

        [HttpGet]
        [Route("Create")]
        [Authorize(Roles = "Manage,Admin")]
        public async Task<IActionResult> Create()
        {
            var categoriesItem = _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.ChildrenCategory)
                .Where(c => c.ParentCategoryId == null && c.Deleted == false)
                .ToList();

            foreach (var item in categoriesItem)
            {
                await GetChildrenCategory(item);
            }

            categoriesItem.Insert(0, new Category() { Id = null, Name = "Không có thể loại cha" });
            ViewData["AllCategories"] = new SelectList(CreateCategoryList(categoriesItem), "Id", "Name");
            return View();
        }


        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = "Manage,Admin")]
        public async Task<IActionResult> Create([Bind("Name,Slug,Description,ParentCategoryId")] Category category)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(category.Slug))
                {
                    category.Slug = AppUtilities.GenerateSlug(category.Name);
                    int slugIndex = 1;
                    while (!(await IsValidSlug(category)))
                    {
                        category.Slug = category.Slug + slugIndex;
                    }
                }
                else if (!(await IsValidSlug(category)))
                {
                    _logger.LogInformation(category.Slug);
                    ModelState.AddModelError("", "Địa chỉ liên kết không hợp lệ, vui lòng nhập địa chỉ khác!");
                }
                else
                {
                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();
                    return Redirect("Manage");
                }
            }

            var categoriesItem = _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.ChildrenCategory)
                .Where(c => c.ParentCategoryId == null && c.Deleted == false)
                .ToList();

            foreach (var item in categoriesItem)
            {
                await GetChildrenCategory(item);
            }

            categoriesItem.Insert(0, new Category() { Id = null, Name = "Không có thể loại cha" });
            ViewData["AllCategories"] = new SelectList(CreateCategoryList(categoriesItem), "Id", "Name");
            return Redirect("/Categories/Manage");
        }

        async Task<bool> IsValidSlug(Category category, bool newCategory = true)
        {
            if (newCategory)
                return !(await _context.Categories.AnyAsync(c => c.Slug == category.Slug));

            else
                return !(await _context.Categories.AnyAsync(c => c.Slug == category.Slug && c.Id != category.Id));
        }

        [HttpGet]
        [Route("Manage/{id}")]
        [Authorize(Roles = "Manage,Admin")]
        public async Task<IActionResult> Edit([FromRoute] string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(Guid.Parse(id));
            if (category == null)
            {
                return NotFound();
            }
            var rootCategories = _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .Include(c => c.ChildrenCategory)
                .Include(c => c.ParentCategory)
                .ToList();
            foreach (var item in rootCategories)
            {
                await GetChildrenCategory(item);
            }
            rootCategories.Insert(0, new Category() { Id = null, Name = "Không có thể loại cha", ParentCategory = null, ChildrenCategory = null });

            ViewData["AllCategories"] = new SelectList(CreateCategoryList(rootCategories, 0), "Id", "Name");
            return View(category);
        }

        List<Category> CreateCategoryList(List<Category> categories, int level = 0)
        {
            var categoryList = new List<Category>();
            foreach (var category in categories)
            {
                string prefix = level == 0 ? "" : string.Concat(Enumerable.Repeat("⎹\xA0\xA0\xA0\xA0\xA0", level - 1)) + "⎹__ ";
                var newCategory = new Category() { Id = category.Id, Name = prefix + category.Name };
                categoryList.Add(newCategory);
                if (category.ChildrenCategory != null && category.ChildrenCategory.Count > 0)
                {
                    var newChildrenCategories = CreateCategoryList(category.ChildrenCategory.ToList(), level + 1);
                    categoryList.AddRange(newChildrenCategories);
                }
            }
            return categoryList;
        }

        [HttpPost]
        [Route("Manage/{id}")]
        [Authorize(Roles = "Manage,Admin")]
        public async Task<IActionResult> Edit([FromRoute] string id, Category category)
        {
            category.Id = Guid.Parse(id);
            var des = category.Description;
            if (ModelState.IsValid)
            {
                var existCategory = await _context.Categories
                    .Include(c => c.ChildrenCategory)
                    .Include(c => c.ParentCategory)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == Guid.Parse(id));


                if (existCategory == null) 
                    return NotFound();

                if (category.ParentCategoryId == null)
                {
                    category.ParentCategoryId = null;
                    category.ParentCategory = null;
                }
                else
                {
                    await GetChildrenCategory(existCategory);
                    if (!(await IsValidParentCategory(existCategory, (Guid)category.ParentCategoryId)))
                    {
                        ModelState.AddModelError("ParentCategoryId", "Danh mục cha không hợp lệ");

                        var rootCategories = _context.Categories
                            .Where(c => c.ParentCategoryId == null)
                            .Include(c => c.ChildrenCategory)
                            .Include(c => c.ParentCategory)
                            .ToList();

                        foreach (var item in rootCategories)
                        {
                            await GetChildrenCategory(item);
                        }
                        rootCategories.Insert(0, new Category() { Id = null, Name = "Không có thể loại cha", ParentCategory = null, ChildrenCategory = null });

                        ViewData["AllCategories"] = new SelectList(CreateCategoryList(rootCategories, 0), "Id", "Name");

                        return View(category);
                    }

                }    

                try
                {
                    _context.ChangeTracker.Clear();
                    _context.Categories.Update(category);
                    await _context.SaveChangesAsync();
                    return Redirect("/Categories/Manage");
                }
                catch(Exception ex)
                {
                    ModelState.AddModelError("", "Không thành công!");
                }

            }
            ViewData["ParenCategoryId"] = new SelectList(_context.Categories, "Id", "Description", category.ParentCategoryId);
            return View(category);
        }

        async Task<bool> IsValidParentCategory(Category category, Guid id)
        {
            if (category.Id == id) return false;
            else if (category.ChildrenCategory == null || category.ChildrenCategory.Count == 0) return true;
            foreach (var child in category.ChildrenCategory)
            {
                if (id == child.Id) return false;
                if (!(await IsValidParentCategory(child, id))) return false;
            }
            return true;
        }


        [HttpPost]
        [Route("Delete")]
        [Authorize(Roles = "Manage,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var category = await _context.Categories
                .Include(c => c.ChildrenCategory)
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Id == Guid.Parse(id));
            if (category == null) return NotFound();

            foreach (var child in category.ChildrenCategory)
            {
                child.ParentCategory = category.ParentCategory;
            }

            category.Deleted = true;
            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return RedirectToAction("Manage");
        }

        public async Task GetChildrenCategory(Category ParentCategoies)
        {
            if (ParentCategoies.ChildrenCategory != null && ParentCategoies.ChildrenCategory.Count() > 0)
            {
                ICollection<Category> newChildrenCategory = new List<Category>();
                foreach (var category in ParentCategoies.ChildrenCategory)
                {
                    var newChildCategory = await _context.Categories
                        .Include(c => c.ParentCategory)
                        .Include(c => c.ChildrenCategory)
                        .FirstOrDefaultAsync(c => c.Id == category.Id && c.Deleted == false);

                    await GetChildrenCategory(newChildCategory);
                    newChildrenCategory.Add(newChildCategory);
                }
                ParentCategoies.ChildrenCategory = newChildrenCategory;
            }
        }

        public void GetChildrenCategory(Category ParentCategoy, List<Category> AllCategories)
        {
            if (ParentCategoy.ChildrenCategory != null && ParentCategoy.ChildrenCategory.Count() > 0)
            {
                List<Category> newChildrenCategories = new List<Category>();
                foreach (var child in ParentCategoy.ChildrenCategory)
                {
                    foreach (var category in AllCategories)
                    {
                        if (category.Id == child.Id && category.Deleted == false)
                        {
                            newChildrenCategories.Add(category);
                            AllCategories.Remove(category);
                            GetChildrenCategory(category, AllCategories);
                        }
                    }
                }
                ParentCategoy.ChildrenCategory = newChildrenCategories;
            }
        }

        [HttpGet("/the-loai/{slug}")]
        public async Task<IActionResult> Details(
            string slug,
            [FromQuery(Name = "page")] int page = 1,
            [FromQuery(Name = "pageSize")] int pageSize = 10,
            [FromQuery(Name = "type")] string type = "bai-viet"
        )
        {
            int countItems = 0;
            var category = await _context.Categories
                .Include(c => c.ChildrenCategory)
                .FirstOrDefaultAsync(c => c.Slug == slug);
            await GetChildrenCategory(category);
            var childCateIds = GetCategoriesId(category);
            if (type == "bai-viet")
            {
                var posts = await _context.Posts
                    .Include(p => p.Thumbnail)
                    .Include(p => p.Author)
                    .Where(p => p.CategoryId != null && childCateIds.Contains((Guid)p.CategoryId) && p.Deleted == false && p.Status == 1)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                category.Posts = posts;
                category.Documents = null;
                countItems = _context.Posts.Count(p => p.CategoryId != null && childCateIds.Contains((Guid)p.CategoryId) && p.Deleted == false);
            }
            else if (type == "tai-lieu")
            {
                var documents = await _context.Documents
                    .Include(p => p.Author)
                    .Where(p => p.CategoryId != null && childCateIds.Contains((Guid)p.CategoryId) && p.Deleted == false)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                category.Documents = documents;
                category.Posts = null;
                countItems = _context.Documents.Count(p => p.CategoryId != null && childCateIds.Contains((Guid)p.CategoryId) && p.Deleted == false);
            }
            ViewData["maxPage"] = countItems % pageSize == 0 ? countItems / pageSize : countItems / pageSize + 1;
            ViewData["currentPage"] = page;
            if (category == null) return NotFound();
            return View(category);
        }

    }
}
