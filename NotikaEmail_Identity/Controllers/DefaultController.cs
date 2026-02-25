using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Models;
using NotikaEmail_Identity.Services.MessageServices;
using System.Threading.Tasks;

namespace NotikaEmail_Identity.Controllers
{
    [Authorize]
    public class DefaultController(IMessageService _messageService,
                                   UserManager<AppUser> _userManager) : Controller
    {
       
        public async Task<IActionResult> Inbox()
        {

            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var messages=await _messageService.GetAllFiterWithSenderAsync(x=>x.ReceiverId==user.Id);
            return View(messages);
        }


        
        public async Task<IActionResult> SendBox()
        {

            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var messages = await _messageService.GetAllFiterWithSenderAsync(x => x.SenderId == user.Id);
            return View(messages);


        }

        public async Task<IActionResult> ViewEmail(int id)
        {

            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var message = await _messageService.GetByIdWithSenderAsync(id);

            return View(message);
        }


        public async Task<IActionResult> ReceiverViewEmail(int id)
        {

            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var message = await _messageService.GetByIdWithReceiverAsync(id);

            return View(message);

        }








    }
}
