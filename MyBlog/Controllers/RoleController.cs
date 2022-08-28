using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyBlog.Models;
using MyBlog.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    public class RoleController : Controller
    {
        private readonly UserManager<User> _UserManager;
        private readonly SignInManager<User> _SignInManager;
        private readonly ILogger<RoleController> _Logger;
        private readonly AppDbContext _Context;
        private readonly RoleManager<IdentityRole> _RoleManager;

        public RoleController(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<RoleController> logger, AppDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _UserManager = userManager;
            _SignInManager = signInManager;
            _Logger = logger;
            _Context = context;
            _RoleManager = roleManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var r = await _RoleManager.Roles.OrderBy(c => c.Name).ToListAsync();
            var roles = new List<ViewRole>();
            foreach (var _r in r)
            {
                var claims = await _RoleManager.GetClaimsAsync(_r);
                var claimStrings = claims.Select(c => c.Type + '=' + c.Value);
                var role = new ViewRole() { Id = _r.Id, Name = _r.Name, Claims = claimStrings.ToArray() };
                roles.Add(role);
            }
            return View(roles);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(ViewCreateRole model)
        {
            if (ModelState.IsValid)
            {
                var newRole = new IdentityRole(model.Name);
                var result = await _RoleManager.CreateAsync(newRole);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost("{roleId}"), ActionName("Edit")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string roleId, ViewEditRole model)
        {
            if (roleId == null) return NotFound("Không tìm thấy Role");
            var role = _RoleManager.FindByIdAsync(roleId);
            if (role == null) return NotFound("Không tìm thấy role");
            if (ModelState.IsValid)
            {

            }
            return RedirectToAction("Index");
        }
    }
}
