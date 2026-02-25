using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NotikaEmail_Identity.DTOs.MessageDtos;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Models;
using NotikaEmail_Identity.Services.CategoryServices;
using NotikaEmail_Identity.Services.MessageServices;
using System.Threading.Tasks;

namespace NotikaEmail_Identity.Controllers
{
    [Authorize]
    public class DefaultController(IMessageService _messageService,
                                   UserManager<AppUser> _userManager,
                                   ICategoryService _categoryService) : Controller
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
            var messageToUpdate = await _messageService.GetByIdAsync(id);
            if (!messageToUpdate.IsRead)
            {
                messageToUpdate.IsRead = true;
                await _messageService.UpdateAsync(messageToUpdate);
            }

            var message = await _messageService.GetByIdWithReceiverAsync(id);

            return View(message);
        }


        public async Task<IActionResult> ReceiverViewEmail(int id)
        {


            var message = await _messageService.GetByIdWithReceiverAsync(id);

            return View(message);

        }


        private async Task GetCategories()
        {
            var categories=await _categoryService.GetAllAsync();

            ViewBag.categories = (from category in categories
                                  select new SelectListItem
                                  {
                                      Text = category.Name,
                                      Value = category.Id.ToString()


                                  }).ToList();




        }




        
        public async Task<IActionResult> SendMessage()
        {
           await  GetCategories();
            return View();


        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(SendMessageViewModel dto)
        {
             await  GetCategories();

            var sender = await _userManager.FindByNameAsync(User.Identity.Name);


            

            var receiver = await _userManager.FindByEmailAsync(dto.ReceiverEmail);

            if(receiver is null)
            {
                ModelState.AddModelError("", "Bu maile sahip bir mail bulunamadı");
                return View(dto);
            }


            var message = new CreateMessageDto()
            {
                Subject = dto.Subject,
                MessageDetail = dto.MessageDetail,
                ReceiverId = receiver.Id,
                SenderId = sender.Id,
                SendDate = DateTime.Now,
                CategoryId = dto.MessageCategoryId,
                IsRead = false
            };


            await _messageService.CreateAsync(message);
            return RedirectToAction("SendBox");



        }




    }
}
