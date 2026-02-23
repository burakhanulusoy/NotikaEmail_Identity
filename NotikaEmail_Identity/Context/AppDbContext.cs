using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NotikaEmail_Identity.Entities;

namespace NotikaEmail_Identity.Context
{
    public class AppDbContext:IdentityDbContext<AppUser,AppRole,int>
    {

        public AppDbContext(DbContextOptions options):base(options)
        {
            

        }

    }
}
