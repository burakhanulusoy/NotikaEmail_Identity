using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace NotikaEmail_Identity.Entities
{
    public class AppUser:IdentityUser<int>
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string? ImageUrl { get; set; }
        public string? City { get; set; }
        public string? TwitterLink { get; set; }
        public string? GitHubLink { get; set; }
        public string? LinkedlnLink { get; set; }
        public string? AboutMe { get; set; }
        public string? Job { get; set; }
        public DateTime? CreatedDate { get; set; }

        public int? ActivationCode { get; set; }



        public IList<Message> SentMessages { get; set; }
        public IList<Message> ReceivedMessages { get; set; }



    }
}
