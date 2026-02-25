using AutoMapper;
using NotikaEmail_Identity.DTOs.CategoryDtos;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Repositories.CategoryRepositories;
using System.Linq.Expressions;

namespace NotikaEmail_Identity.Services.CategoryServices
{
    public class CategoryService(ICategoryRepository _categoryRepository,
                                 IMapper _mapper ) : ICategoryService
    {
        public async Task CreateAsync(CreateCategoryDto dto)
        {
            var category = _mapper.Map<Category>(dto);

            await _categoryRepository.CreateAsync(category);




        }

        public async Task DeleteAsync(int id)
        {
           
            await _categoryRepository.DeleteAsync(id);


        }

        public async Task<List<ResultCategoryDto>> GetAllAsync()
        {
           var categories=await _categoryRepository.GetAllAsync();
            return _mapper.Map<List<ResultCategoryDto>>(categories);

        }

        public async Task<UpdateCategoryDto> GetByIdAsync(int id)
        {
          
            var category=await _categoryRepository.GetByIdAsync(id);
            return _mapper.Map<UpdateCategoryDto>(category);

        }

        public async Task<List<ResultCategoryDto>> GetFilterAsync(Expression<Func<Category, bool>> filter)
        {
            var categories = await _categoryRepository.GetAllFiterAsync(filter);
            return _mapper.Map<List<ResultCategoryDto>>(categories);
        }

        public async Task UpdateAsync(UpdateCategoryDto dto)
        {
            var catgeory= _mapper.Map<Category>(dto);

            await _categoryRepository.UpdateAsync(catgeory);


        }
    }
}
