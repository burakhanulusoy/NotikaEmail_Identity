using NotikaEmail_Identity.DTOs.CategoryDtos;
using NotikaEmail_Identity.Entities.Common;
using System.Linq.Expressions;

namespace NotikaEmail_Identity.Repositories.GenericRepositories
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {

        Task<List<TEntity>> GetAllAsync();
        Task<TEntity> GetByIdAsync(int id);
        Task DeleteAsync(int id);
        Task UpdateAsync(TEntity entity);
        Task CreateAsync(TEntity entity);
        Task<List<TEntity>> GetAllFiterAsync(Expression<Func<TEntity, bool>> filter);




    }
}
