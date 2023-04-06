using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyBlog.Models;
using MyBlog.Service;
using MyBlog.Utilities;
using Nest;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System;

namespace MyBlog.Controllers
{
    [Route("/Statistic")]
    [ApiController]
    public class StatisticController : Controller
    {
        private readonly AppDbContext _Context;
        private readonly ILogger<StatisticController> _Logger;

        public StatisticController(AppDbContext context, ILogger<StatisticController> logger)
        {
            _Context = context;
            _Logger = logger;
        }

        [HttpGet]
        [Route("document")]
        public async Task<BaseResponse<List<Statistic>>> GetDocumentStatistic([FromQuery] string field)
        {
            IQueryable<Statistic> query = field switch
            {
                "name" => _Context.Documents.GroupBy(p => p.Name).Select(g => new Statistic { Code = g.Key, Value = g.Count() }),
                "category" => from c in _Context.Categories
                                            join p in _Context.Documents on c.Id equals p.CategoryId into cp
                                            from pn in cp.DefaultIfEmpty()
                                            select new { Id = c.Id, Name = c.Name, Document = pn } into cp
                                            group cp by new { cp.Id, cp.Name } into gcpn
                                            select new Statistic { Code = gcpn.Key.Id.ToString(), Name = gcpn.Key.Name, Value = gcpn.Count(g => g.Document != null) },
                "author" => _Context.Documents.Include(p => p.Author).GroupBy(p => new { p.Author.Id, p.Author.FirstName, p.Author.LastName }).Select(g => new Statistic { Code = g.Key.Id, Name = $"{g.Key.FirstName} {g.Key.LastName}", Value = g.Count() }),
                "status" => _Context.Documents.GroupBy(p => p.Status).Select(g => new Statistic { Code = g.Key.ToString(), Name = g.Key == 0 ? "Chờ kiểm duyệt" : (g.Key == 1 ? "Đạt yêu cầu" : "Không đạt yêu cầu"), Value = g.Count() }),
                "year" => _Context.Documents.GroupBy(p => p.CreatedAt.Year).Select(g => new Statistic { Code = g.Key.ToString(), Name = g.Key.ToString(), Value = g.Count() }),
                "month" => _Context.Documents.GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month }).Select(g => new Statistic { Code = g.Key.ToString(), Name = $"{g.Key.Month}/{g.Key.Year}", Value = g.Count() }),
                "date" => _Context.Documents.GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month, p.CreatedAt.Day }).Select(g => new Statistic { Code = g.Key.ToString(), Name = $"{g.Key.Day}/{g.Key.Month}/{g.Key.Year}", Value = g.Count() }),

                _ => _Context.Documents.GroupBy(p => "all").Select(g => new Statistic { Code = g.Key, Value = g.Count() })
            };
            List<Statistic> statistics = (await query.ToListAsync()).OrderBy(c => c.Name).ToList();
            return new BaseResponse<List<Statistic>>
            {
                Code = 0,
                message = "",
                Data = statistics
            };
        }

        [HttpGet]
        [Route("category")]
        public async Task<BaseResponse<List<Statistic>>> GetCategoryStatistic([FromQuery] string field)
        {
            IQueryable<Statistic> query = field switch
            {
                "name" => _Context.Categories.GroupBy(p => p.Name).Select(g => new Statistic { Code = g.Key, Value = g.Count() }),
                "parent" => _Context.Categories.Include(c=>c.ParentCategory).GroupBy(p => new {p.ParentCategory.Id, p.ParentCategory.Name}).Select(g => new Statistic { Code = g.Key.Id.ToString(), Name = g.Key.Name, Value = g.Count() }),
                "deleted" => _Context.Categories.GroupBy(p => p.Deleted).Select(g => new Statistic { Code = g.Key.ToString(), Value = g.Count() }),
                _ => _Context.Documents.GroupBy(p => "all").Select(g => new Statistic { Code = g.Key, Value = g.Count() })
            };
            List<Statistic> statistics = await query.ToListAsync();
            return new BaseResponse<List<Statistic>>
            {
                Code = 0,
                message = "",
                Data = statistics
            };
        }

        [HttpGet]
        [Route("censorShip")]
        public async Task<BaseResponse<List<Statistic>>> GetCensorshipStatistic([FromQuery] string field)
        {
            IQueryable<Statistic> query = field switch
            {
                "document" => _Context.Censorships.Include(c => c.Document)
                    .GroupBy(p => new { p.Document.Id, p.Document.Name } )
                    .Select(g => new Statistic { Code = g.Key.Id.ToString(), Name = g.Key.Name, Value = g.Count() }),
                "post" => _Context.Censorships.Include(c => c.Post)
                    .GroupBy(p => new { p.Post.Id, p.Post.Title } )
                    .Select(g => new Statistic { Code = g.Key.Id.ToString(), Name = g.Key.Title, Value = g.Count() }),
                "status" => _Context.Censorships.GroupBy(p => p.Status).Select(g => new Statistic { Code = g.Key.ToString(), Name = g.Key == 1 ? "Đạt yêu cầu" : "Không đạt yêu cầu", Value = g.Count() }),
                "reason" => _Context.Censorships.Include(c=>c.Reason).GroupBy(p => new {p.Reason.Id, p.Reason.Name}).Select(g => new Statistic { Code = g.Key.Id.ToString(), Name= string.IsNullOrEmpty(g.Key.Name)?"Đạt yêu cẩu": g.Key.Name, Value = g.Count() }),
                "year" => _Context.Censorships.GroupBy(p => p.CreatedAt.Year).Select(g => new Statistic { Code = g.Key.ToString(), Name= g.Key.ToString(), Value = g.Count() }),
                "month" => _Context.Censorships.GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month }).Select(g => new Statistic { Code = g.Key.ToString(), Name= $"{g.Key.Month}/{g.Key.Year}", Value = g.Count() }),       
                _ => _Context.Censorships.GroupBy(p => "all").Select(g => new Statistic { Code = g.Key, Value = g.Count() })
            }; ; ;
            List<Statistic> statistics = await query.ToListAsync();
            return new BaseResponse<List<Statistic>>
            {
                Code = 0,
                message = "",
                Data = statistics
            };
        }

        [HttpGet]
        [Route("User")]
        public async Task<BaseResponse<List<Statistic>>> GetUserStatistic([FromQuery] string field)
        {
            IQueryable<Statistic> query = field switch
            {
                "gender" => _Context.Users.GroupBy(p => p.Gender).Select(g => new Statistic { Code = g.Key.ToString(), Name = g.Key == true ? "Name" : "Nữ", Value = g.Count() }),
                "status" => _Context.Users.GroupBy(p => p.EmailConfirmed).Select(g => new Statistic { Code = g.Key.ToString(), Name = g.Key==true?"Đã kích hoạt": "Chưa kích hoạt", Value = g.Count() }),
                "year" => _Context.Users.GroupBy(p => p.CreatedAt.Year).Select(g => new Statistic { Code = g.Key.ToString(), Name = g.Key.ToString(), Value = g.Count() }),
                "month" => _Context.Users.GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month }).Select(g => new Statistic { Code = g.Key.ToString(), Name = $"{g.Key.Month}/{g.Key.Year}", Value = g.Count() }),
                "date" => _Context.Users.GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month, p.CreatedAt.Day }).Select(g => new Statistic { Code = g.Key.ToString(), Name = $"{g.Key.Day}/{g.Key.Month}/{g.Key.Year}", Value = g.Count() }),
                _ => _Context.Users.GroupBy(p => "all").Select(g => new Statistic { Code = g.Key, Value = g.Count() })
            };
            List<Statistic> statistics = (await query.ToListAsync()).OrderBy(u=>u.Name).ToList();
            return new BaseResponse<List<Statistic>>
            {
                Code = 0,
                message = "",
                Data = statistics
            };
        }

        [HttpGet]
        [Route("Post")]
        public async Task<BaseResponse<List<Statistic>>> GetPostStatistic([FromQuery] string field)
        {
            IQueryable<Statistic> query = field switch
            {
                "title" => _Context.Posts.GroupBy(p => p.Title).Select(g => new Statistic { Code = g.Key, Value = g.Count() }),
                "category" => from c in _Context.Categories
                              join p in _Context.Posts on c.Id equals p.CategoryId into cp
                              from pn in cp.DefaultIfEmpty()
                              select new { Id = c.Id, Name = c.Name, Post = pn} into cp
                              group cp by new {cp.Id, cp.Name} into gcpn
                              select new Statistic { Code = gcpn.Key.Id.ToString(), Name = gcpn.Key.Name, Value = gcpn.Count(g=>g.Post != null) },
                "author" => _Context.Posts.Include(p => p.Author).GroupBy(p => new { p.Author.Id, p.Author.FirstName, p.Author.LastName }).Select(g => new Statistic { Code = g.Key.Id, Name = $"{g.Key.FirstName} {g.Key.LastName}", Value = g.Count() }),
                "status" => _Context.Posts.GroupBy(p => p.Status).Select(g => new Statistic { Code = g.Key.ToString(), Name = g.Key == 0 ? "Chờ kiểm duyệt" : (g.Key == 1 ? "Đạt yêu cầu" : "Không đạt yêu cầu"), Value = g.Count() }),
                "year" => _Context.Posts.GroupBy(p => p.CreatedAt.Year).Select(g => new Statistic { Code = g.Key.ToString(), Name = g.Key.ToString(), Value = g.Count() }),
                "month" => _Context.Posts.GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month }).Select(g => new Statistic { Code = g.Key.ToString(), Name = $"{g.Key.Month}/{g.Key.Year}", Value = g.Count() }),
                "date" => _Context.Posts.GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month, p.CreatedAt.Day }).Select(g => new Statistic { Code = g.Key.ToString(), Name = $"{g.Key.Day}/{g.Key.Month}/{g.Key.Year}", Value = g.Count() }),
                _ => _Context.Posts.GroupBy(p => "all").Select(g => new Statistic { Code = g.Key, Value = g.Count() })
            };
            List<Statistic> statistics = (await query.ToListAsync()).OrderBy(c => c.Name).ToList();
            return new BaseResponse<List<Statistic>>
            {
                Code = 0,
                message = "",
                Data = statistics
            };
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
