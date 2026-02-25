using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Repositories.GenericRepositories;
using System.Linq.Expressions;

namespace NotikaEmail_Identity.Repositories.MessageRepositories
{
    public interface IMessageRepository:IGenericRepository<Message>
    {

        Task CloseModifiedForUpdate(Message message);

        Task<List<Message>> GetAllByCategoryAndAppUserAsync();

        Task<List<Message>> GetAllFiterWithSenderAsync(Expression<Func<Message, bool>> filter);

        Task<Message> GetByIdWithReceiverAsync(int id);
        Task<Message> GetByIdWithSenderAsync(int id);

    }
}
