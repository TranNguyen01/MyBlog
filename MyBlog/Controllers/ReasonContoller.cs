using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyBlog.Models;
using MyBlog.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    [Route("/Reason")]
    [ApiController]
    public class ReasonContoller : ControllerBase
    {
        private readonly AppDbContext _DbContext;
        private readonly ILogger<ReasonContoller> _Logger;
        private readonly UserManager<User> _UserManager;

        public ReasonContoller(AppDbContext dbContext, ILogger<ReasonContoller> logger, UserManager<User> userManager)
        {
            _DbContext = dbContext;
            _Logger = logger;
            _UserManager = userManager;
        }

        public async Task<BaseResponse<List<Reason>>> GetList()
        {
            var query = await _DbContext.Reasons.ToListAsync();
            return new BaseResponse<List<Reason>>
            {
                Code = 1,
                message = "",
                Data = query
            };
        }

        [HttpPost]
        public async Task<BaseResponse<Reason>> Create([FromBody] Reason obj)
        {
            _DbContext.Reasons.Add(obj);
            await _DbContext.SaveChangesAsync();
            return new BaseResponse<Reason>
            {
                Code = 1,
                message = "",
                Data = obj
            };
        }
    }
}
