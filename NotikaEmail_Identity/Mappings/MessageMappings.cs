using AutoMapper;
using NotikaEmail_Identity.DTOs.CategoryDtos;
using NotikaEmail_Identity.DTOs.MessageDtos;
using NotikaEmail_Identity.Entities;

namespace NotikaEmail_Identity.Mappings
{
    public class MessageMappings:Profile
    {

        public MessageMappings()
        {
            CreateMap<Message, ResultMessageDto>().ReverseMap();
            CreateMap<Message, UpdateMessageDto>().ReverseMap();
            CreateMap<Message, CreateCategoryDto>().ReverseMap();



        }


    }
}
