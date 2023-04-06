
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using MyBlog.Models;
using MyBlog.Models.ViewModels;
using MyBlog.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    [Route("/Censorship")]
    public class CensorshipController : Controller
    {
        private readonly AppDbContext _DbContext;
        private readonly ILogger<CensorshipController> _Logger;
        private readonly UserManager<User> _UserManager;

        public CensorshipController(UserManager<User> userManager, ILogger<CensorshipController> looger, AppDbContext context)
        {
            _UserManager = userManager;
            _Logger = looger;
            _DbContext = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPostOfCensor([FromQuery] [DefaultValue(1)] int pageIndex, 
            [FromQuery][DefaultValue(1000)] int pageSize,
            [FromQuery][DefaultValue("id")] string sortBy,
            [FromQuery][DefaultValue("asc")] string sortOrder,
            [FromQuery] int? status, [FromQuery] string categoryId,
            [FromQuery] string authorId, [FromQuery] string fromDate,
            [FromQuery] string toDate
        )
        {
            try
            {
                var user = await _UserManager.GetUserAsync(User);
                if (user == null) return Redirect("/login");

                var query = from usr in (
                                from us in _DbContext.Users
                                where us.Id == user.Id
                                select us)
                            join c in _DbContext.CensorCategories on usr.Id equals c.UserId
                            join p in _DbContext.Posts on c.CategoryId equals p.CategoryId
                            join ct in _DbContext.Categories on p.CategoryId equals ct.Id
                            join u in _DbContext.Users on p.AuthorId equals u.Id
                            join pt in _DbContext.Photos on p.Id equals pt.PostId
                            select new Post { 
                                Id = p.Id,
                                Title = p.Title,
                                Description = p.Description,
                                Slug = p.Slug,
                                Content = p.Content,
                                CategoryId = p.CategoryId,
                                Category = p.Category,
                                AuthorId = p.AuthorId,
                                Author = new User
                                {
                                    Id = p.AuthorId,
                                    FirstName = u.FirstName,
                                    LastName = u.LastName,
                                    Email = u.Email,
                                },
                                CreatedAt = p.CreatedAt,
                                LastUpdatedAt = p.LastUpdatedAt,
                                Status = p.Status

                            };
                if (!string.IsNullOrEmpty(categoryId))
                {
                    query = query.Where(q => q.CategoryId == Guid.Parse(categoryId));
                }

                if (status!= null)
                {
                    query = query.Where(q => q.Status == (int)status);
                }

                if (!string.IsNullOrEmpty(authorId))
                {
                    query = query.Where(q => q.AuthorId == authorId);
                }

                if (!string.IsNullOrEmpty(fromDate))
                {
                    query = query.Where(q => q.CreatedAt.Date >= DateTime.Parse(fromDate).Date);
                }

                if (!string.IsNullOrEmpty(toDate))
                {
                    query = query.Where(q => q.CreatedAt.Date <= DateTime.Parse(toDate).Date);
                }

                query = sortBy switch
                {
                    "title"=>sortOrder=="asc"?query.OrderBy(c=>c.Title): query.OrderByDescending(c=>c.Title),
                    "description" => sortOrder == "asc" ? query.OrderBy(c => c.Description) : query.OrderByDescending(c => c.Description),
                    "categoryId" => sortOrder == "asc" ? query.OrderBy(c => c.CategoryId) : query.OrderByDescending(c => c.CategoryId),
                    "authorId" => sortOrder == "asc" ? query.OrderBy(c => c.AuthorId) : query.OrderByDescending(c => c.AuthorId),
                    "createdAt" => sortOrder == "asc" ? query.OrderBy(c => c.CreatedAt) : query.OrderByDescending(c => c.CreatedAt),
                    "status" => sortOrder == "asc" ? query.OrderBy(c => c.Status) : query.OrderByDescending(c => c.Status),
                    _ => sortOrder == "asc" ? query.OrderBy(c => c.Id) : query.OrderByDescending(c => c.Id),
                };

                PaginatedList<Post> data = await PaginatedList<Post>.CreatePaginatedList(query.Distinct(), pageIndex, pageSize);
                return View("PostCensor", data);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex.Message);
                return NotFound();
            }
        }

        [HttpGet]
        [Route("Post/{id}")]
        public async Task<IActionResult> ApprovePost(string id)
        {
            try
            {
                Guid postId = Guid.Parse(id);
                Post post = await _DbContext.Posts
                    .Include(p => p.Category)
                    .Include(p => p.Author)
                    .Include(p => p.Thumbnail)
                    .FirstOrDefaultAsync(p => p.Id == postId);
                if (post == null)
                    return NotFound();
                return View(post);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex.Message);
                return NotFound();
            }
        }

        [HttpGet]
        [Route("Document/{id}")]
        public async Task<IActionResult> ApproveDocument(string id)
        {
            try
            {
                Guid documentId = Guid.Parse(id);
                Document document = await _DbContext.Documents
                    .Include(d => d.Author)
                    .Include(d => d.Category)
                    .FirstOrDefaultAsync(p => p.Id == documentId);
                if (document == null)
                    return NotFound();
                return View(document);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<BaseResponse<ViewCensorship>> Approve([FromBody] ViewCensorship model)
        {
            if (!ModelState.IsValid)
            {
                return new BaseResponse<ViewCensorship>()
                {
                    Code = BaseConst.ResponseStatusCode.NOT_FOUND,
                    message = "Không hợp lệ",
                    Data = null
                };
            }
            if (model.PostId == null && model.DocumentId == null)
            {
                return new BaseResponse<ViewCensorship>()
                {
                    Code = BaseConst.ResponseStatusCode.NOT_FOUND,
                    message = "Id không hợp lệ",
                    Data = null
                };
            }

            User user = await _UserManager.GetUserAsync(User);
           
            if (user == null) { 
                return new BaseResponse<ViewCensorship>()
                {
                    Code = BaseConst.ResponseStatusCode.NOT_FOUND,
                    message = "Người dùng không tồn tại",
                    Data = null
                };
            }

            if (model.PostId != null)
            {
                Guid postId = Guid.Parse(model.PostId);
                Post post = await _DbContext.Posts.Include(c=>c.Author).Include(c=>c.Category).FirstOrDefaultAsync(c=>c.Id == postId);
                if (post == null)
                {
                    return new BaseResponse<ViewCensorship>()
                    {
                        Code = BaseConst.ResponseStatusCode.NOT_FOUND,
                        message = "Bài viết không tồn tại",
                        Data = null
                    };
                };
                if(post.Status == model.Status)
                {
                    return new BaseResponse<ViewCensorship>()
                    {
                        Code = BaseConst.ResponseStatusCode.NOT_FOUND,
                        message = model.Status == 1 ? "Bài viết đã được chấp thuận trước đó!" : "Bài viết đã bị từ chối trước đó!",
                        Data = null
                    };
                }
                model.Post = post;
                post.Status = model.Status;
                _DbContext.Entry(post).State = EntityState.Modified;
            }

            if(model.DocumentId != null)
            {
                Guid documentId = Guid.Parse(model.DocumentId);
                Document document = await _DbContext.Documents.Include(c => c.Author).Include(c => c.Category).FirstOrDefaultAsync(d=>d.Id == documentId);
                if (document == null)
                {
                    return new BaseResponse<ViewCensorship>()
                    {
                        Code = BaseConst.ResponseStatusCode.NOT_FOUND,
                        message = "Tài liệu không tồn tại",
                        Data = null
                    };
                }
                if (document.Status == model.Status)
                {
                    return new BaseResponse<ViewCensorship>()
                    {
                        Code = BaseConst.ResponseStatusCode.NOT_FOUND,
                        message = model.Status == 1 ? "Tài liệu đã được chấp thuận trước đó!" : "Tài liệu đã bị từ chối trước đó!",
                        Data = null
                    };
                }
                model.Document = document;
                document.Status = model.Status;
                _DbContext.Entry(document).State = EntityState.Modified;
            }

            model.UserId = user.Id;
            Censorship censorShip = new Censorship(model);
            censorShip.CreatedAt = DateTime.Now;
            censorShip.UpdatedAt = DateTime.Now;

            Notify notify = new Notify()
            {
                Title = model.Status == -1 ? "Bài viết bị từ chối": "Bài viết đã được chấp thuận",
                Content = model.Status == -1 ? $"Bài viết bị từ chối bởi \"{user.LastName} {user.FirstName}\"" : $"Bài viết đã được chấp thuận bởi \"{user.LastName} {user.FirstName}\"",
                UserId = user.Id,
                Seen = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _DbContext.Censorships.Add(censorShip);
            _DbContext.Notify.Add(notify);
            await _DbContext.SaveChangesAsync();
            return new BaseResponse<ViewCensorship>()
            {
                Code = BaseConst.ResponseStatusCode.SUCCESS,
                message = "",
                Data = censorShip.Export()
            };
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Update(string id, Censorship model)
        {
            try
            {
                model.Id = Guid.Parse(id);
                if (!ModelState.IsValid)
                    return View(model);

                if (model.PostId == null && model.DocumentId == null)
                    return NotFound();

                User user = await _UserManager.GetUserAsync(User);
                if (user == null) return NotFound();

                if (model.PostId != null)
                {
                    Guid postId = (Guid)model.PostId;
                    Post post = await _DbContext.Posts.FindAsync(postId);
                    if (post == null) return NotFound();
                    post.Status = model.Status;
                    _DbContext.Entry(post).State = EntityState.Modified;
                }

                if (model.DocumentId != null)
                {
                    Guid documentId = (Guid)model.DocumentId;
                    Document document = await _DbContext.Documents.FindAsync(documentId);
                    if (document == null) return NotFound();
                    document.Status = model.Status;
                    _DbContext.Entry(document).State = EntityState.Modified;
                }

                model.UserId = user.Id;
                model.UpdatedAt = DateTime.Now;

                Notify notify = new Notify()
                {
                    Title = model.Status == -1 ? "Bài viết bị từ chối" : "Bài viết đã được chấp thuận",
                    Content = model.Status == -1 ? $"Bài viết bị từ chối bởi \"{user.LastName} {user.FirstName}\"" : $"Bài viết đã được chấp thuận bởi \"{user.LastName} {user.FirstName}\"",
                    UserId = user.Id,
                    Seen = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _DbContext.Censorships.Update(model);
                _DbContext.Notify.Add(notify);
                await _DbContext.SaveChangesAsync();
                return Redirect("/");
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex.Message);
                return View(model);
            }
        }
    }
}
