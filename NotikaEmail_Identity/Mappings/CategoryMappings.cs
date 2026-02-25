using AutoMapper;
using NotikaEmail_Identity.DTOs.CategoryDtos;
using NotikaEmail_Identity.Entities;

namespace NotikaEmail_Identity.Mappings
{
    public class CategoryMappings:Profile
    {
        public CategoryMappings()
        {
            
            CreateMap<Category,ResultCategoryDto>().ReverseMap();
            CreateMap<Category,UpdateCategoryDto>().ReverseMap();
            CreateMap<Category,CreateCategoryDto>().ReverseMap();


        }

    }
}
