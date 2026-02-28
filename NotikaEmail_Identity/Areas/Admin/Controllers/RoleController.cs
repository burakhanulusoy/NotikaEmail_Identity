using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.RoleNames;
using System.Threading.Tasks;

namespace NotikaEmail_Identity.Areas.Admin.Controllers
{
    [Area(Roles.Admin)]
    public class RoleController(RoleManager<AppRole> _roleManager,UserManager<AppUser> _useerManager) : Controller
    {
        public async Task<IActionResult> Index()
        {

            var roles = await _roleManager.Roles.ToListAsync();

            return View(roles);
        }

        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(AppRole role)
        {

            var result = await _roleManager.CreateAsync(role);

            if(!result.Succeeded)
            {
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);

                }
                return View(role);

            }

            return RedirectToAction(nameof(Index));


        }


        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            await _roleManager.DeleteAsync(role);
            return RedirectToAction(nameof(Index));

        }


        public async Task<IActionResult> UpdateRole(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            return View(role);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(AppRole role)
        {

            await _roleManager.UpdateAsync(role);
            return RedirectToAction($"{nameof(Index)}");


        }




    }
}
