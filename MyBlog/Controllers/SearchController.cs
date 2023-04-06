using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBlog.Models;
using MyBlog.Service;
using Nest;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    public class SearchController : Controller
    {
        private readonly IElasticsearch _ESClient;
        private readonly AppDbContext _context;
        public SearchController(AppDbContext context)
        {
            _ESClient = new ElasticSearch();
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _ESClient.CreateIndexAsync<Document>("document");
            return Ok();
        }

        [HttpGet("/IndexAll")]
        public async Task<IActionResult> IndexAllData()
        {
            if (!(_ESClient.CheckExistIndex("document")))
            {
                var indexResult = await _ESClient.CreateIndexAsync<Document>("document");
            }
            if (!(_ESClient.CheckExistIndex("post")))
            {
                var indexResult = await _ESClient.CreateIndexAsync<Post>("post");
            }
            var posts = await _context.Posts.Include(c => c.Category).Include(a => a.Author).ToListAsync();
            foreach(var post in posts)
            {
                var res = await _ESClient.Index(post, "post",  post.Id.ToString());
            }
            return Ok();
        }

        [HttpGet("/IndexPost")]
        public async Task<IActionResult> IndexPostData()
        {
            if (!(_ESClient.CheckExistIndex("post")))
            {
                var indexResult = await _ESClient.CreatePostIndexAsync();
            }
            var posts = await _context.Posts.Include(c => c.Category).Include(a => a.Author).ToListAsync();
            foreach (var post in posts)
            {
                var res = await _ESClient.Index(post, "post", post.Id.ToString());
            }
            return Ok();
        }

        [HttpGet("/autoComplete")]
        public async Task<IActionResult> AutoComplete([FromQuery] string searchKey)
        {
            var result = await _ESClient.AutoComplete<Post>("post", searchKey, 1000);
            return Ok(result);
        }


        [HttpGet("/search")]
        public async Task<IActionResult> Search([FromQuery] string searchKey)
        {
            var result = await _ESClient.Search<Post>("post", searchKey, 1, 10);
            return Ok(result);
        }
        //public SearchController(IElasticsearch postESClient, IElasticsearch documentESClient)
        //{
        //    PostESClient = postESClient;
        //    DocumentESClient = documentESClient;
        //}

        //public async Task<IActionResult> AutoComplete(string searchKey)
        //{
        //    ESClient.AutoComplete("")
        //}
    }
}
