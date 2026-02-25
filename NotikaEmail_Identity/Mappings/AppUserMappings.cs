using AutoMapper;
using NotikaEmail_Identity.DTOs.UserDtos;
using NotikaEmail_Identity.Entities;

namespace NotikaEmail_Identity.Mappings
{
    public class AppUserMappings:Profile
    {
        public AppUserMappings()
        {


            CreateMap<AppUser,ResultUserDto>().ReverseMap();
            CreateMap<AppUser,UpdateUserDto>().ReverseMap();
            CreateMap<AppUser,CreateUserDto>().ReverseMap();


        }








    }
}
