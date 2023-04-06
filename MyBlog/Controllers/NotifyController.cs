using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyBlog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    [Route("/Notify")]
    public class NotifyController : Controller
    {
        private readonly AppDbContext _DbContext;
        private readonly ILogger<NotifyController> _Logger;
        private readonly UserManager<User> _UserManager;

        public NotifyController(AppDbContext dbContext, ILogger<NotifyController> logger, UserManager<User> userManager)
        {
            _DbContext = dbContext;
            _Logger = logger;
            _UserManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifyOfMine(int pageIndex = 1, int pageSize = 10)
        {
            var user = await _UserManager.GetUserAsync(User);
            if (user == null)
                return Redirect("/Login");
            List<Notify> notifies = await _DbContext.Notify
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n=>n.CreatedAt)
                .ToListAsync();
            return Ok(notifies);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> SeenNotify(string id)
        {
            try
            {
               Guid notifyId = Guid.Parse(id);
                var user = await _UserManager.GetUserAsync(User);
                if (user == null) 
                    return Redirect("/Login");
                Notify notify = await _DbContext.Notify.FindAsync(notifyId);
                if (notify == null)
                    return NotFound();
                notify.Seen = true;
                _DbContext.Entry(notify).State = EntityState.Modified;
                var result = await _DbContext.SaveChangesAsync();
                if (result <= 0)
                    return NotFound();
                return Ok(notify);

            }catch(Exception ex)
            {
                _Logger.LogError(ex.Message);
                return Redirect("/");
            }

        }


    }
}
