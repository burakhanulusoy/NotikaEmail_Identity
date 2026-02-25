using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Repositories.GenericRepositories;

namespace NotikaEmail_Identity.Repositories.UserRepositories
{
    public interface IUserRepository:IGenericRepository<AppUser>
    {
    }
}
