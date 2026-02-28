using Microsoft.EntityFrameworkCore;
using NotikaEmail_Identity.Context;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Repositories.GenericRepositories;
using System.Linq.Expressions;

namespace NotikaEmail_Identity.Repositories.MessageRepositories
{
    public class MessageRepository : GenericRepository<Message>, IMessageRepository
    {
        public MessageRepository(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        public async Task CloseModifiedForUpdate(Message message)
        {
            _context.Update(message);

            _context.Entry(message).Property(x => x.SendDate).IsModified = false;

            await _context.SaveChangesAsync();

        }

        public async Task<List<Message>> GetAllByCategoryAndAppUserAsync()
        {
            
            return await _table.Include(x=>x.Category)
                               .Include(x=>x.Sender)
                               .Include(x=>x.Receiver)
                               .ToListAsync();

        }

        public async Task<List<Message>> GetAllFiterWithSenderAsync(Expression<Func<Message, bool>> filter)
        {
            return await _table.Include(x => x.Category)
                               .Include(x => x.Sender)
                               .Include(x => x.Receiver)
                               .Where(filter)
                               .OrderByDescending(x=>x.Id)
                               .ToListAsync();

        }

        public async Task<Message> GetByIdWithReceiverAsync(int id)
        {

            var message = await _table.Where(x=>x.Id==id).Include(x=>x.Category).Include(x=>x.Receiver).Include(x=>x.Sender).AsNoTracking().FirstOrDefaultAsync();
            return message;
          
            

        }

        public async Task<Message> GetByIdWithSenderAsync(int id)
        {
            var message = await _table.Where(x => x.Id == id).Include(x => x.Sender).Include(x=>x.Receiver).AsNoTracking().FirstOrDefaultAsync();
            return message;
        }

        public async Task<int> GetDontReadMessageCountAsync(int id)
        {
             var count= await _table.Where(x=>x.ReceiverId == id && x.IsRead == false).CountAsync();
            
             return count;



        }

        public async Task<List<Message>> GetLast5DontReadMessageAsync(int id)
        {

            var messages = await _table.Where(x => x.ReceiverId == id && x.IsRead == false)
                .Include(x=>x.Sender)
                .OrderByDescending(x=>x.Id).Take(5).ToListAsync();

            return messages;

        }

        public async Task<List<Message>> PeopleISentMessagesTo(int id)
        {
            var messages = await _table.Where(x=>x.SenderId ==id).Include(x=>x.Receiver).Include(x=>x.Sender).AsNoTracking().ToListAsync();
            var uniqueMessages = messages.DistinctBy(x => x.ReceiverId).ToList();

            return uniqueMessages;

        }
    }
}
