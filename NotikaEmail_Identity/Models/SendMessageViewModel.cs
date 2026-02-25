namespace NotikaEmail_Identity.Models
{
    public class SendMessageViewModel
    {
        public string ReceiverEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public int MessageCategoryId { get; set; }




    }
}
