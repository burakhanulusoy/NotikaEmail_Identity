using System.Linq.Expressions;

namespace NotikaEmail_Identity.Services.GenericServices
{
    public interface IGenericServices<TEntity,TResultDto,TUpdateDto,TCreateDto>
    {

        Task<List<TResultDto>> GetAllAsync();
        Task<TUpdateDto> GetByIdAsync(int id);
        Task CreateAsync(TCreateDto dto);
        Task UpdateAsync(TUpdateDto dto);
        Task DeleteAsync(int id);
        Task<List<TResultDto>> GetFilterAsync(Expression<Func<TEntity, bool>> filter);




    }
}
