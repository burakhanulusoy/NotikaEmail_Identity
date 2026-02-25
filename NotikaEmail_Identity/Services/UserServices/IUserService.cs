using NotikaEmail_Identity.DTOs.UserDtos;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Services.GenericServices;

namespace NotikaEmail_Identity.Services.UserServices
{
    public interface IUserService:IGenericServices<AppUser,ResultUserDto,UpdateUserDto,CreateUserDto>
    {


    }
}
