using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyBlog.Models;
using MyBlog.Models.ViewModels;
using MyBlog.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    [Route("/Report")]
    public class ReportController : Controller
    {
        private readonly AppDbContext _DbContext;
        private readonly ILogger<ReportController> _Logger;
        private readonly UserManager<User> _UserManager;

        public ReportController(AppDbContext dbContext, ILogger<ReportController> logger, UserManager<User> userManager)
        {
            _DbContext = dbContext;
            _Logger = logger;
            _UserManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet]
        [Route("GetList")]
        public async Task<BaseResponse<List<PostReport>>> GetList()
        {
            var query = from i in (
                            from r in _DbContext.Reports
                            group r by r.PostId into rg
                            select new { Id = rg.Key, Count = rg.Count() })
                        join p in _DbContext.Posts on i.Id equals p.Id
                        join c in _DbContext.Categories on p.CategoryId equals c.Id
                        join ph in _DbContext.Photos on p.Id equals ph.PostId
                        join u in _DbContext.Users on p.AuthorId equals u.Id
                        select new PostReport
                        {
                            PostId = p.Id,
                            Post = new Post
                            {
                                Id = p.Id,
                                Title = p.Title,
                                Content = p.Content,
                                CreatedAt = p.CreatedAt,
                                Status = p.Status,
                                Thumbnail = new Photo
                                {
                                    Url = ph.Url
                                },
                                Category = new Category
                                {
                                    Id = c.Id,
                                    Name = c.Name
                                },
                                Author = new User
                                {
                                    FirstName = u.FirstName,
                                    LastName = u.LastName
                                }
                            },
                            Count = i.Count
                        };
            List<PostReport> postReports = await query.ToListAsync();
            return new BaseResponse<List<PostReport>>
            {
                Code = 0,
                Data = postReports,
                message = ""
            };
        }

        [HttpGet]
        [Route("post")]
        public async Task<BaseResponse<PaginatedList<Report>>> GetPostReport(
            [FromQuery][DefaultValue(1)] int pageIndex, [FromQuery][DefaultValue(100)] int pageSize, 
            [FromQuery] string reason = "all", [FromQuery] string post = "all", 
            [FromQuery] string status = "all", [FromQuery] string user = "all", 
            [FromQuery] string sortBy = "id", [FromQuery] string sortOrder = "asc")
        {
            var query  = _DbContext.Reports
                .Include(r => r.Post)
                .Include(r=>r.Reason)
                .Where(r => r.PostId != null);

            if (reason != "all")
            {
                Guid reasonId = Guid.Parse(reason);
                query = query.Where(r => r.ReasonId == reasonId);
            }

            if (post != "all")
            {
                Guid postId = Guid.Parse(post);
                query = query.Where(r => r.PostId == postId);
            }

            if (user != "all")
            {
                query = query.Where(r => r.UserId == user);
            }

            query = status switch
            {
                "reviewed" => query.Where(r => r.Reviewed == true),
                "unReviewed" => query.Where(r => r.Reviewed == false),
                _ => query
            };

            query = sortBy switch
            {
                "postId" => sortOrder == "asc" ? query.OrderBy(r => r.PostId) : query.OrderByDescending(r => r.PostId),
                "reasonId" => sortOrder == "asc" ? query.OrderBy(r => r.ReasonId) : query.OrderByDescending(r => r.ReasonId),
                "createAt" => sortOrder == "asc" ? query.OrderBy(r => r.CreatedAt) : query.OrderByDescending(r => r.CreatedAt),
                "reviewedAt" => sortOrder == "asc" ? query.OrderBy(r => r.ReviewedAt) : query.OrderByDescending(r => r.ReviewedAt),
                "reviewed" => sortOrder == "asc" ? query.OrderBy(r => r.Reviewed) : query.OrderByDescending(r => r.Reviewed),
                "post" => sortOrder == "asc" ? query.OrderBy(r => r.Post.Title) : query.OrderByDescending(r => r.Post.Title),
                "reason" => sortOrder == "asc" ? query.OrderBy(r => r.Reason) : query.OrderByDescending(r => r.Reason),
                _ => sortOrder == "asc" ? query.OrderBy(r => r.Id) : query.OrderByDescending(r => r.Id)
            };

            //List<Report> results = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new BaseResponse<PaginatedList<Report>>
            {
                Code = 1,
                message = "",
                Data = await PaginatedList<Report>.CreatePaginatedList(query, pageIndex, pageSize)
            };
        }

        [HttpGet]
        [Route("document")]
        public async Task<IActionResult> GetDocumentReport(
            [FromQuery] int pageIndex, [FromQuery] int pageSize,
            [FromQuery] string reason = "all", [FromQuery] string document = "all",
            [FromQuery] string status = "all", [FromQuery] string user = "all",
            [FromQuery] string sortBy = "id", [FromQuery] string sortOrder = "asc")
        {
            var query = _DbContext.Reports
                .Include(r => r.Document)
                .Where(r => r.DocumentId != null);

            query = sortBy switch
            {
                "documentId" => sortOrder == "asc" ? query.OrderBy(r => r.DocumentId) : query.OrderByDescending(r => r.DocumentId),
                "reasonId" => sortOrder == "asc" ? query.OrderBy(r => r.ReasonId) : query.OrderByDescending(r => r.ReasonId),
                "createAt" => sortOrder == "asc" ? query.OrderBy(r => r.CreatedAt) : query.OrderByDescending(r => r.CreatedAt),
                "reviewedAt" => sortOrder == "asc" ? query.OrderBy(r => r.ReviewedAt) : query.OrderByDescending(r => r.ReviewedAt),
                "reviewed" => sortOrder == "asc" ? query.OrderBy(r => r.Reviewed) : query.OrderByDescending(r => r.Reviewed),
                "document" => sortOrder == "asc" ? query.OrderBy(r => r.Post.Title) : query.OrderByDescending(r => r.Post.Title),
                "reason" => sortOrder == "asc" ? query.OrderBy(r => r.Reason) : query.OrderByDescending(r => r.Reason),
                _ => sortOrder == "asc" ? query.OrderBy(r=>r.Id) : query.OrderByDescending(r => r.Id)
            };

            if (reason != "all")
            {
                Guid reasonId = Guid.Parse(reason);
                query = query.Where(r => r.ReasonId == reasonId);
            }

            if (document != "all")
            {
                Guid postId = Guid.Parse(document);
                query = query.Where(r => r.PostId == postId);
            }

            if (user != "all")
            {
                query = query.Where(r => r.UserId == user);
            }

            query = status switch
            {
                "reviewed" => query.Where(r => r.Reviewed == true),
                "unReviewed" => query.Where(r => r.Reviewed == false),
                _ => query
            };

            List<Report> results = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return View(results);
        }

        [HttpPost]
        public async Task<BaseResponse<Report>> Create([FromBody] ViewReport obj)
        {
            var user = await _UserManager.GetUserAsync(User);
            if (user == null)
                return new BaseResponse<Report>
                {
                    Code = -1,
                    message = "Chưa đăng nhập",
                    Data = null
                };
            obj.Id = null;
            obj.UserId = user.Id;
            Report report = new Report();
            report.Parse(obj);
            report.CreatedAt = DateTime.Now;
            report.Reviewed = false;

            _DbContext.Reports.Add(report);
            _ = await _DbContext.SaveChangesAsync();

            return new BaseResponse<Report>
            {
                Code = 0,
                message = "",
                Data = report
            };
        }
    }
}
