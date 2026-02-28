using NotikaEmail_Identity.Context;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Repositories.GenericRepositories;

namespace NotikaEmail_Identity.Repositories.UserRepositories
{
    public class UserRepository : GenericRepository<AppUser>, IUserRepository
    {
        public UserRepository(AppDbContext appDbContext) : base(appDbContext)
        {
        }


    }
}
