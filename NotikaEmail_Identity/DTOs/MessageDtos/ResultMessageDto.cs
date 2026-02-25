using NotikaEmail_Identity.DTOs.CategoryDtos;
using NotikaEmail_Identity.DTOs.UserDtos;
using NotikaEmail_Identity.Entities;

namespace NotikaEmail_Identity.DTOs.MessageDtos
{
    public class ResultMessageDto
    {

        public int Id { get; set; }
        public ResultUserDto Receiver { get; set; }
        public int ReceiverId { get; set; }
        public ResultUserDto Sender { get; set; }
        public int SenderId { get; set; }


        public string Subject { get; set; }
        public DateTime SendDate { get; set; }
        public string MessageDetail { get; set; }

        public bool IsRead { get; set; }


        public ResultCategoryDto Category { get; set; }
        public int CategoryId { get; set; }



    }
}
