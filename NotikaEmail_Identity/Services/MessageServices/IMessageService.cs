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

    }
}
