using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyBlog.Models;
using MyBlog.Models.ViewModels;
using MyBlog.Service;
using MyBlog.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    public class DocumentController : Controller
    {
        private readonly AppDbContext _Context;
        private readonly ILogger<DocumentController> _Logger;
        private readonly UserManager<User> _UserManager;
        private readonly Cloudinary _cloudinary;
        private readonly IMinIOService _minIOService;
        private readonly IElasticsearch _ESClient;

        public DocumentController(AppDbContext context, ILogger<DocumentController> logger, UserManager<User> userManager, Cloudinary cloudinary, IMinIOService minIOService, IElasticsearch eSClient)
        {
            _Context = context;
            _Logger = logger;
            _UserManager = userManager;
            _cloudinary = cloudinary;
            _minIOService = minIOService;
            _ESClient = new ElasticSearch();
        }

        [HttpGet("/document/create")]
        public IActionResult Create()
        {
            ViewData["AllCategories"] = new SelectList(_Context.Categories.Where(c => c.Deleted == false).ToList(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ViewDocumentCrt document)
        {
            if (!ModelState.IsValid)
            {
                ViewData["AllCategories"] = new SelectList(_Context.Categories.Where(c => c.Deleted == false).ToList(), "Id", "Name");
                return View(document);
            }

            string fileName = new Guid().ToString();

            var user = await _UserManager.GetUserAsync(User);
            var newDocument = new Document
            {
                Name = document.Name,
                ContentType = document.File.ContentType,
                Length = document.File.Length,
                Description = document.Description,
                AuthorID = user.Id,
                CategoryId = Guid.Parse(document.CategoryId),
                CreatedAt = DateTime.UtcNow,
                BucketName = "document",
                FileName = fileName,
                OriginFileName = document.File.FileName,
                Deleted = false,
                Dowloaded = 0,
            };

            try
            {
                await _minIOService.PutProject("document", document.File.OpenReadStream(), fileName, document.File.ContentType);
                if (!(_ESClient.CheckExistIndex("document1")))
                {
                    var indexResult = await _ESClient.CreatePostIndexAsync();
                }
                _Context.Documents.Add(newDocument);
                _Context.SaveChanges();
                var result = await _ESClient.Index(newDocument, "document1", newDocument.Id.ToString());
                return Redirect("/MyDocuments");
                //return Redirect("/");
            }
            catch (Exception ex)
            {
                ViewData["AllCategories"] = new SelectList(_Context.Categories.Where(c => c.Deleted == false).ToList(), "Id", "Name");
                ViewData["Error"] = "Lỗi hệ thống";
                return View(document);
            }
        }



        [HttpGet]
        public async Task<IActionResult> Download(string id)
        {
            var document = await _Context.Documents.FirstOrDefaultAsync(d => d.Id == new Guid(id));
            if (document == null)
                return NotFound();
            document.Dowloaded += 1;
            _Context.Entry(document).State = EntityState.Modified;
            await _Context.SaveChangesAsync();

            var fileStream = await _minIOService.GetObject(document.BucketName, document.FileName);
            if (fileStream == null || fileStream.Length == 0)
                return NotFound();

            return File(fileStream, document.ContentType, document.Name + Path.GetExtension(document.OriginFileName));
        }

        [HttpGet("/document/{id}")]
        public async Task<IActionResult> Detail(string id)
        {
            var document = await _Context.Documents
                .Include(d => d.Category)
                .Include(d => d.Author)
                .FirstOrDefaultAsync(d => d.Id == new Guid(id));

            if (document == null)
                return NotFound();
            var viewDocument = new ViewDocument();
            viewDocument.Parse(document);

            var tempUrl = await _minIOService.PresignedGetObject(document.BucketName, document.FileName);
            viewDocument.TempUrl = tempUrl;

            return View(viewDocument);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateDocument(ViewUpdateDocument document)
        {
            if (!ModelState.IsValid)
            {
                ViewData["AllCategories"] = new SelectList(_Context.Categories.Where(c => c.Deleted == false).ToList(), "Id", "Name");
                return View(document);
            }

            Document oldDocument = await _Context.Documents.FirstOrDefaultAsync(d => d.Id == document.Id);
            if (oldDocument == null)
                return NotFound();

            var user = await _UserManager.GetUserAsync(User);
            if (user == null)
                return NotFound();
            else if (user.Id != oldDocument.AuthorID)
                return NotFound();

            Category category = await _Context.Categories.FirstOrDefaultAsync(c => c.Id == document.CategoryId);
            if (category == null)
                return NotFound();

            oldDocument.Name = document.Name;
            oldDocument.Description = document.Description;
            oldDocument.CategoryId = document.CategoryId;

            _Context.Entry(oldDocument).State = EntityState.Modified;
            await _Context.SaveChangesAsync();

            oldDocument.Category = category;
            oldDocument.Author = user;
            return Redirect("/MyDocuments");
        }

    
        public async Task<IActionResult> Delete(string id)
        {
            Document document = await _Context.Documents.FirstOrDefaultAsync(d => d.Id == new Guid(id));
            if (document == null)
                return NotFound();

            var user = await _UserManager.GetUserAsync(User);
            if (user == null || user.Id != document.AuthorID)
                return NotFound();

            document.Deleted = true;
            _Context.Entry(document).State = EntityState.Modified;
            await _Context.SaveChangesAsync();
            return Redirect("/MyDocuments");
        }
    }
}
