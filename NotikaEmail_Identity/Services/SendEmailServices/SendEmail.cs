using MailKit.Net.Smtp;
using MimeKit;

namespace NotikaEmail_Identity.Services.SendEmailServices
{
    public class SendEmail : ISendEmail
    {
        void ISendEmail.SendEmail(string email, int code)
        {
            MimeMessage mimeMessage = new MimeMessage();

            MailboxAddress mailboxAddressFrom = new MailboxAddress("NotikaApp", "burakhanulusoy18@gmail.com");//kimden gidecek 

            mimeMessage.From.Add(mailboxAddressFrom);

            MailboxAddress mailboxAddressTo = new MailboxAddress("User",email);

            mimeMessage.To.Add(mailboxAddressTo);


            var bodyBuilder = new BodyBuilder();

            bodyBuilder.TextBody = "Hesabınızı doğrulamak için gelen onay kodu :" + code;

            mimeMessage.Body = bodyBuilder.ToMessageBody();

            mimeMessage.Subject = "Mail notika aktivasyon kodu";

            SmtpClient client = new SmtpClient();//transfer protokulı

            client.Connect("smtp.gmail.com", 587, false);

            client.Authenticate("burakhanulusoy18@gmail.com", "vgmsjisxwqiyyflm");

            client.Send(mimeMessage);

            client.Disconnect(true);

        }
    }
}
