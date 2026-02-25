using NotikaEmail_Identity.DTOs.CategoryDtos;

namespace NotikaEmail_Identity.DTOs.MessageDtos
{
    public class CreateMessageDto
    {
        public int ReceiverId { get; set; }

        public int SenderId { get; set; }


        public string Subject { get; set; }
        public DateTime SendDate { get; set; }
        public string MessageDetail { get; set; }

        public bool IsRead { get; set; }


        public ResultCategoryDto Category { get; set; }
        public int CategoryId { get; set; }

    }
}
