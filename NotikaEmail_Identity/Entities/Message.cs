using NotikaEmail_Identity.Entities.Common;

namespace NotikaEmail_Identity.Entities
{
    public class Message:BaseEntity
    {
        public AppUser Receiver { get; set; }
        public int ReceiverId { get; set; }
        public AppUser Sender { get; set; }
        public int SenderId { get; set; }


        public string Subject { get; set; }
        public DateTime SendDate { get; set; }
        public string MessageDetail { get; set; }

        public bool IsRead { get; set; }


        public Category Category { get; set; }
        public int CategoryId { get; set; }

        //eklenen dosya yolu tutacak bakalım yapabılırsek
        public string? AttachedFilePath { get; set; }

        //public bool IsGarbage { get; set; }
        //public bool IsDeleted { get; set; }
        //public bool IsArchive { get; set; }




    }
}
