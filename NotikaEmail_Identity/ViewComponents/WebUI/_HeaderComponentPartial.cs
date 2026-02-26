using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Services.MessageServices;
using System.Threading.Tasks;

namespace NotikaEmail_Identity.ViewComponents.WebUI
{
    public class _HeaderComponentPartial(IMessageService _messageService,UserManager<AppUser> _userManager):ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var messages = await _messageService.GetLast5DontReadMessageAsync(user.Id);

            ViewBag.messageCount= await _messageService.GetDontReadMessageCountAsync(user.Id);

            return View(messages);
        }



    }
}
