using Microsoft.AspNetCore.Identity;

namespace NotikaEmail_Identity.Entities
{
    public class AppUser:IdentityUser<int>
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string? ImageUrl { get; set; }
        public string? City { get; set; }


    }
}
