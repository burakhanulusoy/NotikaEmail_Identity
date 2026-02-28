using AutoMapper;
using NotikaEmail_Identity.DTOs.MessageDtos;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Repositories.MessageRepositories;
using System.Linq.Expressions;

namespace NotikaEmail_Identity.Services.MessageServices
{
    public class MessageService(IMessageRepository _messageRepository,
                                IMapper _mapper) : IMessageService
    {




        public async Task CreateAsync(CreateMessageDto dto)
        {
            
            var message=_mapper.Map<Message>(dto);
            await _messageRepository.CreateAsync(message);

           
        }

        public async Task DeleteAsync(int id)
        {
            await _messageRepository.DeleteAsync(id);
        }

        public async Task<List<ResultMessageDto>> GetAllAsync()
        {
            var messages=await _messageRepository.GetAllAsync();
            return _mapper.Map<List<ResultMessageDto>>(messages);


        }

        public async Task<List<ResultMessageDto>> GetAllByCategoryAndAppUserAsync()
        {
            var messages=await _messageRepository.GetAllByCategoryAndAppUserAsync();
            return _mapper.Map<List<ResultMessageDto>>(messages);
        }

        public async Task<List<ResultMessageDto>> GetAllFiterWithSenderAsync(Expression<Func<Message, bool>> filter)
        {
            var messages = await _messageRepository.GetAllFiterWithSenderAsync(filter);
            return _mapper.Map<List<ResultMessageDto>>(messages);
        }

        public async Task<UpdateMessageDto> GetByIdAsync(int id)
        {
           
            var message=await _messageRepository.GetByIdAsync(id);
            return _mapper.Map<UpdateMessageDto>(message);

        }

        public async Task<ResultMessageDto> GetByIdWithReceiverAsync(int id)
        {

            var message = await _messageRepository.GetByIdWithReceiverAsync(id);
            return _mapper.Map<ResultMessageDto>(message);

        }

        public async Task<UpdateMessageDto> GetByIdWithSenderAsync(int id)
        {
            var message = await _messageRepository.GetByIdWithSenderAsync(id);
            return _mapper.Map<UpdateMessageDto>(message);
        }

        public Task<int> GetDontReadMessageCountAsync(int id)
        {
            return _messageRepository.GetDontReadMessageCountAsync(id);
        }

        public async Task<List<ResultMessageDto>> GetFilterAsync(Expression<Func<Message, bool>> filter)
        {
            var messages = await _messageRepository.GetAllFiterAsync(filter);
            return _mapper.Map<List<ResultMessageDto>>(messages);

        }

        public async Task<List<ResultMessageDto>> GetLast5DontReadMessageAsync(int id)
        {

            var messages = await _messageRepository.GetLast5DontReadMessageAsync(id);
            return _mapper.Map<List<ResultMessageDto>>(messages);

        }

        public async Task<List<ResultMessageDto>> PeopleISentMessagesTo(int id)
        {

            var messages = await _messageRepository.PeopleISentMessagesTo(id);
            return _mapper.Map<List<ResultMessageDto>>(messages);

        }

        public async Task UpdateAsync(UpdateMessageDto dto)
        {
            var message=_mapper.Map<Message>(dto);
            await _messageRepository.CloseModifiedForUpdate(message);


        }
    }
}
