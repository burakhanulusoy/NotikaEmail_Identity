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


        //Toplam okunmayan mesaj sayısı alanın mesajı receiver
        Task<int> GetDontReadMessageCountAsync(int id);
        //okunmyan son 5 mesajı getir
        Task<List<Message>> GetLast5DontReadMessageAsync(int id);

        // burda sey yaptım kullancın kımlere mesaj attıgı sayfsaı ıın lzım oldu id senderId
        Task<List<Message>> PeopleISentMessagesTo(int id);



    }
}
