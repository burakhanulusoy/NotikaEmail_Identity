using NotikaEmail_Identity.DTOs.CategoryDtos;

namespace NotikaEmail_Identity.Models
{
    public class SendMessageViewModel
    {
        public string ReceiverEmail { get; set; }
        public string Subject { get; set; }
        public string MessageDetail { get; set; }
        public int MessageCategoryId { get; set; }



    }
}
