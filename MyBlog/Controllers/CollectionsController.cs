using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyBlog.Models;
using MyBlog.Models.ViewModels;
using MyBlog.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    public class CollectionsController : Controller
    {
        private readonly AppDbContext _Context;
        private readonly UserManager<User> _UserManager;
        private readonly ILogger<CollectionsController> _Logger;

        private JsonSerializerSettings jsonSetting = new JsonSerializerSettings()
        {
            Formatting = Newtonsoft.Json.Formatting.Indented,
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        public CollectionsController(AppDbContext context, UserManager<User> userManager, ILogger<CollectionsController> logger)
        {
            _Context = context;
            _UserManager = userManager;
            _Logger = logger;
        }

        //[HttpGet]
        //public async Task<IActionResult> MyCollections()
        //{
        //    var user = await _UserManager.GetUserAsync(User);
        //    if (user == null) return NotFound();
        //    try
        //    {

        //        var collections = await GetCollectionsByUserId(user.Id);
        //        //var reusult = JsonConvert.SerializeObject(collections, setting);
        //        return Json(collections, jsonSetting);
        //    }
        //    catch (Exception ex)
        //    {
        //        _Logger.LogError(ex.Message);
        //        return BadRequest(ex.Message);
        //    }
        //}

        [HttpGet("/Collections/MyCollections")]
        public async Task<IActionResult> MyCollections()
        {
            var user = await _UserManager.GetUserAsync(User);
            if (user == null) return NotFound();
            try
            {

                var collections = await GetCollectionsByUserId(user.Id);
                //var reusult = JsonConvert.SerializeObject(collections, setting);
                return View("~/Views/Account/MyCollections.cshtml", collections);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("/Collections/GetAllCollections")]
        public async Task<IActionResult> GetAllCollections()
        {
            var user = await _UserManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var collections = await _Context.Collections
                .Include(c => c.PostCollections)
                .Where(c => c.UserId == user.Id)
                .Select(c => new Collections
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentCollectionsId = c.ParentCollectionsId,
                    CreateAt = c.CreateAt,
                    UpdateAt = c.UpdateAt,
                    PostCollections = c.PostCollections
                })
                .ToListAsync();

            var rootCollections = new List<Collections>();
            foreach (var item in collections)
            {
                if (item.ParentCollectionsId == null)
                {
                    rootCollections.Add(item);
                    GenerateChildrenCollections(item, collections);
                }
            }
            return Json(rootCollections);
        }

        [HttpGet("/Collections/{collectionsId}")]
        public async Task<IActionResult> MyCollectionsDetail(string collectionsId)
        {
            var user = await _UserManager.GetUserAsync(User);
            if (user == null) return NotFound();
            try
            {
                var collections = await GetCollectionsDetail(user.Id, collectionsId);
                //var reusult = JsonConvert.SerializeObject(collections, setting);
                return View("~/Views/Account/CollectionsDetail.cshtml", collections);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        private async Task<ICollection<Collections>> GetCollectionsByUserId(string userId)
        {
            var collections = await _Context.Collections
                .Include(c => c.PostCollections)
                .ThenInclude(pc => pc.Post)
                .Where(c => c.UserId == userId)
                .Select(c => new Collections
                {
                    Id = c.Id,
                    Name = c.Name,
                    UserId = c.UserId,
                    ParentCollectionsId = c.ParentCollectionsId,
                    CreateAt = c.CreateAt,
                    UpdateAt = c.UpdateAt,
                    PostCollections = (from pc in c.PostCollections
                                       select new
                                       PostCollection
                                       {
                                           PostId = pc.PostId,
                                           Post = new Post
                                           {
                                               Thumbnail = pc.Post.Thumbnail,
                                               Title = pc.Post.Title,
                                               Id = pc.Post.Id,
                                               LikesCount = pc.Post.LikesCount,
                                               Slug = pc.Post.Slug
                                           },
                                           CollectionsId = pc.CollectionsId,
                                       }).ToList()
                })
                .ToListAsync();

            var rootCollections = new List<Collections>();
            foreach (var item in collections)
            {
                if (item.ParentCollectionsId == null)
                {
                    rootCollections.Add(item);
                    GenerateChildrenCollections(item, collections);
                }
            }
            return rootCollections;

        }

        private async Task<Collections> GetCollectionsDetail(string userId, string id)
        {
            var collections = await _Context.Collections
                .Include(c => c.ParentCollections)
                .Include(c => c.PostCollections)
                .ThenInclude(pc => pc.Post)
                .Include(c => c.ChildrenCollections)
                .Where(c => c.UserId == userId && c.Id == Guid.Parse(id))
                .Select(c => new Collections
                {
                    Id = c.Id,
                    Name = c.Name,
                    UserId = c.UserId,
                    ParentCollectionsId = c.ParentCollectionsId,
                    ParentCollections = c.ParentCollections,
                    CreateAt = c.CreateAt,
                    UpdateAt = c.UpdateAt,
                    ChildrenCollections = c.ChildrenCollections,
                    PostCollections = (from pc in c.PostCollections
                                       select new
                                       PostCollection
                                       {
                                           PostId = pc.PostId,
                                           Post = new Post
                                           {
                                               Thumbnail = pc.Post.Thumbnail,
                                               Title = pc.Post.Title,
                                               Id = pc.Post.Id,
                                               LikesCount = pc.Post.LikesCount,
                                               Slug = pc.Post.Slug
                                           },
                                           CollectionsId = pc.CollectionsId,
                                       }).ToList()
                })
                .FirstOrDefaultAsync();
            var rootCollection = collections.ParentCollections;
            while (rootCollection != null)
            {
                _Context.Entry(rootCollection).Reference(c => c.ParentCollections).Load();
                rootCollection = rootCollection.ParentCollections;
            }
            return collections;

        }



        private void GenerateChildrenCollections(Collections parentCollections, ICollection<Collections> collections)
        {
            parentCollections.ChildrenCollections = new List<Collections>();
            foreach (var item in collections)
            {
                if (item.ParentCollectionsId == parentCollections.Id)
                {
                    parentCollections.ChildrenCollections.Add(item);
                    GenerateChildrenCollections(item, collections);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(ViewCollections collections)
        {
            var user = await _UserManager.GetUserAsync(User);
            if (user == null) return BadRequest();

            if (ModelState.IsValid)
            {
                var newCollections = new Collections()
                {
                    UserId = user.Id,
                    Name = collections.Name.Trim(),
                    CreateAt = DateTime.Now,
                    UpdateAt = DateTime.Now,
                    ParentCollectionsId = null,
                    ChildrenCollections = new List<Collections>(),
                    PostCollections = new List<PostCollection>()
                };

                if (collections.ParentCollectionsId != null)
                {
                    var isExistCollections = await _Context.Collections.AnyAsync(c => c.Id == Guid.Parse(collections.ParentCollectionsId));
                    if (isExistCollections == false) return BadRequest();
                    else newCollections.ParentCollectionsId = Guid.Parse(collections.ParentCollectionsId);
                }

                try
                {
                    await _Context.Collections.AddAsync(newCollections);
                    await _Context.SaveChangesAsync();

                    return Json(newCollections);
                }
                catch (Exception ex)
                {
                    _Logger.LogError(ex.Message);
                    return BadRequest();
                }
            }

            return BadRequest();

        }

        [HttpPatch]
        public async Task<IActionResult> Rename(string id, string name)
        {
            var user = await _UserManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {
                var collections = await _Context.Collections.FindAsync(Guid.Parse(id));

                if (collections.UserId != user.Id) return NotFound();
                collections.Name = name.Trim();

                try
                {
                    await _Context.SaveChangesAsync();
                    return Json(new
                    {
                        success = true,
                        message = "Edit success",
                        collections = collections
                    });
                }
                catch (Exception ex)
                {
                    _Logger.LogError(ex.Message);
                    return Json(new
                    {
                        success = false,
                        message = "Edit error",
                        collections = (Collections)null
                    });
                }

            }

            return BadRequest();
        }

        [HttpPost("/collections/{collectionsId}/post")]
        public async Task<IActionResult> AddPost(string collectionsId, string postId)
        {
            var user = await _UserManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {
                var collections = await _Context.Collections.FindAsync(Guid.Parse(collectionsId));
                if (collections == null) return NotFound();
                if (collections.UserId != user.Id) return NotFound();

                var post = await _Context.Posts.FindAsync(Guid.Parse(postId));
                if (post == null) return NotFound();

                var newPostCollections = new PostCollection()
                {
                    CollectionsId = collections.Id,
                    PostId = post.Id
                };

                _Context.PostCollections.Add(newPostCollections);
                collections.PostCollections.Add(newPostCollections);

                var result = new PostCollection()
                {
                    PostId = newPostCollections.PostId,
                    CollectionsId = newPostCollections.CollectionsId
                };

                try
                {
                    await _Context.SaveChangesAsync();
                    return Json(new
                    {
                        success = true,
                        message = "Add success",
                        postCollections = result
                    }, jsonSetting
                    );
                }
                catch (Exception ex)
                {
                    _Logger.LogError(ex.Message);
                    return Json(new
                    {
                        success = false,
                        message = "Add Error",
                        postCollections = (Collections)null
                    });
                }
            }
            return NotFound();
        }

        [HttpDelete("/collections/{collectionsId}/post")]
        public async Task<IActionResult> RemovePost(string collectionsId, string postId)
        {
            var user = await _UserManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {
                var postCollections = await _Context.PostCollections
                    .FirstOrDefaultAsync(pc => pc.CollectionsId == Guid.Parse(collectionsId) && pc.PostId == Guid.Parse( postId));
                if (postCollections == null) return NotFound();

                var collections = await _Context.Collections.FindAsync(postCollections.CollectionsId);
                if (collections == null) return NotFound();
                else if (collections.UserId != user.Id) return BadRequest();

                _Context.PostCollections.Remove(postCollections);

                try
                {
                    await _Context.SaveChangesAsync();
                    return Json(new
                    {
                        success = true,
                        message = "Success",
                        collections = (Collections)null
                    });
                }
                catch (Exception ex)
                {
                    _Logger.LogError(ex.Message);
                    return Json(new
                    {
                        success = true,
                        message = "Error",
                        collections = (Collections)null
                    });
                }
            }
            return BadRequest();
        }


        [HttpPost("/collections/{collectionsId}/document")]
        public async Task<IActionResult> AddDocument(int collectionsId, string documentId)
        {
            var user = await _UserManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {
                var collections = await _Context.Collections.FindAsync(collectionsId);
                if (collections == null) return NotFound();
                if (collections.UserId != user.Id) return NotFound();

                var document = await _Context.Documents.FirstOrDefaultAsync(d => d.Id == new Guid(documentId));
                if (document == null) return NotFound();

                var documentCollection = new DocumentCollection()
                {
                    CollectionsId = collections.Id,
                    DocumentId = document.Id
                };

                _Context.DocumentCollections.Add(documentCollection);
                collections.DocumentCollections.Add(documentCollection);

                var result = new DocumentCollection()
                {
                    DocumentId = documentCollection.DocumentId,
                    CollectionsId = documentCollection.CollectionsId
                };

                try
                {
                    await _Context.SaveChangesAsync();
                    return Json(
                    new BaseResponse<DocumentCollection>
                    {
                        Code = 1,
                        message = "Add success",
                        Data = result
                    },
                    jsonSetting
                    );
                }
                catch (Exception ex)
                {
                    _Logger.LogError(ex.Message);
                    return Json(new BaseResponse<DocumentCollection>
                    {
                        Code = 0,
                        message = "Add Error",
                        Data = null
                    });
                }
            }
            return NotFound();
        }

        [HttpDelete("/collections/{collectionsId}/document")]
        public async Task<IActionResult> RemoveDocument(string collectionsId, string documentId)
        {
            var user = await _UserManager.GetUserAsync(User);
            if (user == null)
                return Json(new BaseResponse<Collections> { Code = 3, message = "Chưa đăng nhập", Data = null });

            if (ModelState.IsValid)
            {
                var documentCollections = await _Context.DocumentCollections
                    .FirstOrDefaultAsync(pc => pc.CollectionsId == Guid.Parse(collectionsId) && pc.DocumentId == new Guid(documentId));
                if (documentCollections == null) return NotFound();

                var collections = await _Context.Collections.FindAsync(documentCollections.CollectionsId);
                if (collections == null) return NotFound();
                else if (collections.UserId != user.Id) return BadRequest();

                _Context.DocumentCollections.Remove(documentCollections);

                try
                {
                    await _Context.SaveChangesAsync();
                    return Json(new BaseResponse<Collections>
                    {
                        Code = 1,
                        message = "Success",
                        Data = null
                    });
                }
                catch (Exception ex)
                {
                    _Logger.LogError(ex.Message);
                    return Json(new BaseResponse<Collections>
                    {
                        Code = 0,
                        message = "Error",
                        Data = null
                    });
                }
            }
            return BadRequest();
        }

        [HttpDelete("/Collections/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _UserManager.GetUserAsync(User);
            if (user == null) return NotFound();
            var collections = await _Context.Collections.FindAsync(Guid.Parse(id));
            var parentId = collections.ParentCollectionsId;
            if (collections == null) return NotFound();
            else if (collections.UserId != user.Id) return BadRequest();

            await DeleteCollections(collections);
            await _Context.SaveChangesAsync();
            return Json(new
            {
                success = true,
                message = "Success",
                collections = (Collections)null,
                redirectUrl = $"/Collections/{parentId}"
            });
        }

        private async Task DeleteCollections(Collections collections)
        {
            _Context.Collections.Remove(collections);

            var children = await _Context.Collections
                .Include(c => c.ChildrenCollections)
                .Where(c => c.ParentCollectionsId == collections.Id)
                .ToListAsync();

            foreach (var item in children)
            {
                _Context.Collections.Remove(item);
                foreach (var child in item.ChildrenCollections)
                {
                    await DeleteCollections(child);
                }
            }
        }
    }
}
