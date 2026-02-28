using NotikaEmail_Identity.DTOs.CategoryDtos;
using NotikaEmail_Identity.DTOs.MessageDtos;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Services.GenericServices;
using System.Linq.Expressions;

namespace NotikaEmail_Identity.Services.MessageServices
{
    public interface IMessageService:IGenericServices<Message,ResultMessageDto,UpdateMessageDto,CreateMessageDto>
    {
        Task<List<ResultMessageDto>> GetAllByCategoryAndAppUserAsync();
        Task<List<ResultMessageDto>> GetAllFiterWithSenderAsync(Expression<Func<Message, bool>> filter);

        Task<ResultMessageDto> GetByIdWithReceiverAsync(int id);
        Task<UpdateMessageDto> GetByIdWithSenderAsync(int id);


        //Toplam okunmayan mesaj sayısı alanın mesajı receiver
        Task<int> GetDontReadMessageCountAsync(int id);
        //okunmyan son 5 mesajı getir
        Task<List<ResultMessageDto>> GetLast5DontReadMessageAsync(int id);


        // burda sey yaptım kullancın kımlere mesaj attıgı sayfsaı ıın lzım oldu id senderId
        Task<List<ResultMessageDto>> PeopleISentMessagesTo(int id);


    }
}
