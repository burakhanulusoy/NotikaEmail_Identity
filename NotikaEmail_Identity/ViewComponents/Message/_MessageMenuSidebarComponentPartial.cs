using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Services.MessageServices;
using System.Threading.Tasks;

namespace NotikaEmail_Identity.ViewComponents.Message
{
    public class _MessageMenuSidebarComponentPartial(IMessageService _messageService,UserManager<AppUser> _userManager):ViewComponent
    {

        public async Task<IViewComponentResult> InvokeAsync()
        {

            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            int dontReadMessageCount = await _messageService.GetDontReadMessageCountAsync(user.Id);

            ViewBag.count = dontReadMessageCount;

            return View();
        }




    }
}
