using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyBlog.Models;
using MyBlog.Utilities;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    [Route("/Censor")]
    public class CensorController : Controller
    {
        private readonly AppDbContext _DbContext;
        private readonly ILogger<CensorController> _Logger;
        private readonly UserManager<User> _UserManager;

        public CensorController(AppDbContext dbContext, ILogger<CensorController> logger, UserManager<User> userManager)
        {
            _DbContext = dbContext;
            _Logger = logger;
            _UserManager = userManager;
        }

        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAllCensor([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var query = from user in _DbContext.Users
                        join userRole in _DbContext.UserRoles on user.Id equals userRole.UserId
                        join role in _DbContext.Roles on userRole.RoleId equals role.Id
                        where role.Name.ToLower() == "censor"
                        select user;
            List<User> users = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return Ok(new BaseResponse<List<User>>
            {
                Code = 1,
                message = "",
                Data = users
            });
        }

        [HttpGet]
        [Route("ByCategory/{id}")]
        public async Task<IActionResult> GetAllCensorByCategory([FromRoute] string id, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            Guid categoryId = Guid.Parse(id);
            var query = from censorCategory in _DbContext.CensorCategories
                        join user in _DbContext.Users on censorCategory.UserId equals user.Id
                        join userRole in _DbContext.UserRoles on user.Id equals userRole.UserId
                        join role in _DbContext.Roles on userRole.RoleId equals role.Id
                        where role.Name.ToLower() == "censor" && censorCategory.CategoryId == categoryId
                        select user;

            List<User> users = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return Ok(new BaseResponse<List<User>>
            {
                Code = 1,
                message = "",
                Data = users
            });
        }

        [HttpGet]
        [Route("Valid")]
        public async Task<BaseResponse<List<User>>> GetValidCensor([FromQuery] string catId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                //var query = from user1 in (
                //                from user in _DbContext.Users
                //                join cenCat in _DbContext.CensorCategories on user.Id equals cenCat.UserId into u
                //                from cenCate in u.DefaultIfEmpty()
                //                where cenCate == null || cenCate.CategoryId != Guid.Parse(catId)
                //                group cenCate by new
                //                {
                //                    Id = cenCate.User.Id,
                //                    FirstName = cenCate.User.FirstName,
                //                    LastName = cenCate.User.LastName,
                //                    BirthDate = cenCate.User.BirthDate,
                //                    Gender = cenCate.User.Gender,
                //                    Email = cenCate.User.Email,
                //                } into gCenCate
                //                select new User
                //                {
                //                    Id = gCenCate.Key.Id,
                //                    FirstName = gCenCate.Key.FirstName,
                //                    LastName = gCenCate.Key.LastName,
                //                    BirthDate = gCenCate.Key.BirthDate,
                //                    Gender = gCenCate.Key.Gender,
                //                    Email = gCenCate.Key.Email,
                //                })
                //            join userRole in _DbContext.UserRoles on user1.Id equals userRole.UserId
                //            join role in _DbContext.Roles on userRole.RoleId equals role.Id
                //            join ava in _DbContext.Photos on user1.AvatarId equals ava.Id into a
                //            from avatar in a.DefaultIfEmpty()
                //            where role.Name == BaseConst.Role.CENSOR
                //            select new User {
                //                Id = user1.Id, 
                //                FirstName = user1.FirstName, 
                //                LastName = user1.LastName, 
                //                BirthDate = user1.BirthDate, 
                //                Gender = user1.Gender,
                //                Email = user1.Email,
                //                Avatar = new Photo
                //                {
                //                    Url = avatar.Url
                //                },
                //            };

                var exist = from c in _DbContext.Categories
                            join t in _DbContext.CensorCategories on c.Id equals t.CategoryId
                            where c.Id == Guid.Parse(catId)
                            select t.UserId;

                var query = from user1 in (
                                from user in _DbContext.Users
                                join userRole in _DbContext.UserRoles on user.Id equals userRole.UserId
                                join role in _DbContext.Roles on userRole.RoleId equals role.Id
                                where role.Name == BaseConst.Role.CENSOR
                                join cenCat in _DbContext.CensorCategories on user.Id equals cenCat.UserId into u
                                from cenCate in u.DefaultIfEmpty()
                                where (cenCate == null || cenCate.CategoryId != Guid.Parse(catId))
                                select user).Distinct()
                            where !exist.Contains(user1.Id)
                            join ava in _DbContext.Photos on user1.AvatarId equals ava.Id into a
                            from avatar in a.DefaultIfEmpty()
                            select new User
                            {
                                Id = user1.Id,
                                FirstName = user1.FirstName,
                                LastName = user1.LastName,
                                BirthDate = user1.BirthDate,
                                Gender = user1.Gender,
                                Email = user1.Email,
                                Avatar = new Photo
                                {
                                    Url = avatar.Url
                                }
                            };


                List<User> users = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
                return new BaseResponse<List<User>>
                {
                    Code = 1,
                    message = "",
                    Data = users
                };
            }
            catch (Exception ex)
            {

                return new BaseResponse<List<User>>
                {
                    Code = 1,
                    message = ex.Message,
                    Data = null
                };
            }
        }


        [HttpPost]
        public async Task<BaseResponse<CensorCategory>> AddCensorToCategory([FromBody] CensorCategory censor)
        {
            var query = from user in _DbContext.Users
                        join userRole in _DbContext.UserRoles on user.Id equals userRole.UserId
                        join role in _DbContext.Roles on userRole.RoleId equals role.Id
                        where user.Id == censor.UserId && role.Name == BaseConst.Role.CENSOR
                        select new User
                        {
                            Id = user.Id
                        };
            var userFnd = await query.FirstOrDefaultAsync();
            if (userFnd == null)
                return new BaseResponse<CensorCategory>()
                {
                    Code = BaseConst.ResponseStatusCode.NOT_FOUND,
                    message = "Người dùng không tồn tại",
                    Data = null
                };
            var category = await _DbContext.Categories.FirstOrDefaultAsync(c => c.Id == censor.CategoryId);
            if (category == null)
                return new BaseResponse<CensorCategory>()
                {
                    Code = BaseConst.ResponseStatusCode.NOT_FOUND,
                    message = "Thể loại không tồn tại",
                    Data = null
                };
            censor.status = 1;
            censor.CreatedAt = DateTime.Now;
            censor.UpdatedAt = DateTime.Now;
            _DbContext.CensorCategories.Add(censor);
            await _DbContext.SaveChangesAsync();
            return new BaseResponse<CensorCategory>()
            {
                Code = BaseConst.ResponseStatusCode.SUCCESS,
                message = "",
                Data = censor
            };
        }

        [HttpDelete]
        public async Task<BaseResponse<CensorCategory>> RemoveCensorCategory([FromBody] CensorCategory censor)
        {
          
            try
            {
                var censorCat = await _DbContext.CensorCategories.FirstOrDefaultAsync(c => c.UserId == censor.UserId && c.CategoryId == censor.CategoryId);
                if (censorCat == null)
                    return new BaseResponse<CensorCategory>()
                    {
                        Code = BaseConst.ResponseStatusCode.NOT_FOUND,
                        message = "Kiểm duyệt không tồn tại",
                        Data = null
                    };
                _DbContext.CensorCategories.Remove(censorCat);
                await _DbContext.SaveChangesAsync();
                return new BaseResponse<CensorCategory>()
                {
                    Code = BaseConst.ResponseStatusCode.SUCCESS,
                    message = "",
                    Data = null
                };
            }
            catch(Exception ex)
            {
                return new BaseResponse<CensorCategory>()
                {
                    Code = BaseConst.ResponseStatusCode.SYSTEM_ERROR,
                    message = "Lỗi hệ thống!",
                    Data = null
                };
            }
            
        }

        public async Task<IActionResult> Index()
        {
            List<User> users = await (from user in _DbContext.Users
                        join userRole in _DbContext.UserRoles on user.Id equals userRole.UserId
                        join role in _DbContext.Roles on userRole.RoleId equals role.Id
                        where role.Name.ToLower() == "censor"
                        select user).ToListAsync();
            List<Category> categories = await _DbContext.Categories.Where(c => c.Deleted == false).ToListAsync();

            return View();
        }
    }
}
