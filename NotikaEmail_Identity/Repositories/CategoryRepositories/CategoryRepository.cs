using NotikaEmail_Identity.Context;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Repositories.GenericRepositories;

namespace NotikaEmail_Identity.Repositories.CategoryRepositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext appDbContext) : base(appDbContext)
        {
        }
    }
}
