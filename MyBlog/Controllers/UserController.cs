using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBlog.Models;
using MyBlog.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<User> _UserManager;
        private readonly RoleManager<IdentityRole> _RoleManager;
        private readonly AppDbContext _Context;

        public UserController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context)
        {
            _UserManager = userManager;
            _RoleManager = roleManager;
            _Context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var qr = _UserManager.Users.OrderBy(u => u.FirstName);
            const int perPage = 20;
            var totalUser = await qr.CountAsync();
            var totalPage = totalUser / perPage;


            var users = await qr.Skip((page - 1) * perPage)
                .Take(perPage).Select(u => new ViewUserAndRole()
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName
                })
                .ToListAsync();

            foreach (var user in users)
            {
                var r = await _UserManager.GetRolesAsync(user);
                user.Roles = r;
            }

            var roles = await _RoleManager.Roles.OrderBy(c => c.Name).Select(c => c.Name).ToListAsync();
            ViewData["AllRoles"] = roles;
            return View(users);
        }

        [HttpPost("/User/UpdateRole/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRole(string id, List<string> Roles)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _UserManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            if (ModelState.IsValid)
            {
                var oldRoles = await _UserManager.GetRolesAsync(user);
                var deleteRoles = oldRoles.Where(r => !Roles.Contains(r));
                var addRoles = Roles.Where(r => !oldRoles.Contains(r));

                var resultAdd = await _UserManager.AddToRolesAsync(user, addRoles);
                if (!resultAdd.Succeeded)
                    ModelState.AddModelError("", "Không thành công");

                var resultRemove = await _UserManager.RemoveFromRolesAsync(user, deleteRoles);
                if (!resultRemove.Succeeded)
                    ModelState.AddModelError("", "Không thành công");

                return Json(new { success = true, message = "thanh cong" });

            }
            return Json(new { success = false, message = "khong thanh cong" });
        }
    }

}
