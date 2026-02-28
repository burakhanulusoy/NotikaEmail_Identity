using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotikaEmail_Identity.DTOs.UserDtos;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Models;
using NotikaEmail_Identity.RoleNames;
using NotikaEmail_Identity.Services.UserServices;
using System.Threading.Tasks;

namespace NotikaEmail_Identity.Areas.Admin.Controllers
{
    [Area(Roles.Admin)]
    public class UserController(RoleManager<AppRole> _roleManager,UserManager<AppUser> _userManager,IUserService _userService,IMapper _mapper) : Controller
    {
        public async Task<IActionResult> Index()
        {

            var users = await _userManager.Users.ToListAsync();

            var mappedUser = _mapper.Map<List<ResultUserDto>>(users);

            foreach(var user in users)
            {
                var roles = await  _userManager.GetRolesAsync(user);

                mappedUser.Find(x => x.Id == user.Id).Roles = roles;


            }



            return View(mappedUser);
        }

        public async Task<IActionResult> AssignRole(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            var userRoles  = await _userManager.GetRolesAsync(user);

            var allRoles = await _roleManager.Roles.ToListAsync();

            var assignRole = new List<AssignRoleViewModel>();

            foreach(var role in allRoles)
            {

                assignRole.Add(new AssignRoleViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    UserId = user.Id,
                    RoleExists = userRoles.Contains(role.Name)




                });


            }

            ViewBag.UserName = user.Name + " " + user.Surname;

            return View(assignRole);

        }





        [HttpPost]
        public async Task<IActionResult> AssignRole(List<AssignRoleViewModel> model)
        {

            var userId = model.Select(x => x.UserId).FirstOrDefault();

            var user = await _userManager.FindByIdAsync(userId.ToString());

            foreach(var role in model)
            {


                if(role.RoleExists)
                {
                    await _userManager.AddToRoleAsync(user, role.RoleName);
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(user, role.RoleName);

                }




            }



           return RedirectToAction("Index");



        }














    }
}
