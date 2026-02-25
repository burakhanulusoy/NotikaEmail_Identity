using Microsoft.EntityFrameworkCore;
using NotikaEmail_Identity.Context;
using NotikaEmail_Identity.Entities.Common;
using System.Linq.Expressions;

namespace NotikaEmail_Identity.Repositories.GenericRepositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity
    {

        protected readonly AppDbContext _context;
        protected readonly DbSet<TEntity> _table;

        public GenericRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
            _table=_context.Set<TEntity>();

        }


        public async Task CreateAsync(TEntity entity)
        {
            
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();



        }

        public async Task DeleteAsync(int id)
        {
            
            var entity=await _table.FindAsync(id);
            _context.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<TEntity>> GetAllAsync()
        {
            return await  _table.AsNoTracking().ToListAsync();
        }

        public async Task<List<TEntity>> GetAllFiterAsync(Expression<Func<TEntity, bool>> filter)
        {
            
            return await _table.Where(filter).AsNoTracking().ToListAsync();

        }

        public async Task<TEntity> GetByIdAsync(int id)
        {
            var entity = await _table.FindAsync(id);

            if(entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached; //yapma amacım güncellerken takıp edılıyor hatası alammak

            }

            return entity;

        }

        public async Task UpdateAsync(TEntity entity)
        {
            
            _context.Update(entity);

            await _context.SaveChangesAsync();



        }
    }
}
