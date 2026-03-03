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
            var existingEntity = await _context.Messages.FindAsync(message.Id);

            
                // 2. Mapper'dan gelen yeni değerleri, veritabanındaki nesnenin üzerine "giydirelim"
                _context.Entry(existingEntity).CurrentValues.SetValues(message);

                // 3. SendDate'in değişmesini istemediğin kuralı burada işletelim
                _context.Entry(existingEntity).Property(x => x.SendDate).IsModified = false;

                // 4. Değişiklikleri kaydedelim
                await _context.SaveChangesAsync();
           
          
        }

        public async Task<List<Message>> GetAllByCategoryAndAppUserAsync()
        {
            
            return await _table.Include(x=>x.Category)
                               .Include(x=>x.Sender)
                               .Include(x=>x.Receiver)
                               .ToListAsync();

        }

        public async Task<List<Message>> GetAllDrafAsync()
        {

            return await _table.Include(x => x.Category)
                                .Include(x => x.Sender)
                                .Include(x => x.Receiver)
                                .Where(x=>x.IsDraft==true)
                                .Where(x => x.IsDeleted == false)
                                .Where(x => x.IsSpam == false)
                                .OrderByDescending(x => x.Id)
                                .ToListAsync();

        }

        public async Task<List<Message>> GetAllFiterWithReceiverAsync(Expression<Func<Message, bool>> filter)
        {
            return await _table.Include(x => x.Category)
                              .Include(x => x.Sender)
                              .Include(x => x.Receiver)
                              .Where(filter)
                              .Where(x => x.IsDeleted == false)
                              .Where(x => x.IsDraft == false)
                              .Where(x => x.IsSpam == false)
                              .OrderByDescending(x => x.Id)
                              .ToListAsync();
        }

        public async Task<List<Message>> GetAllFiterWithSenderAsync(Expression<Func<Message, bool>> filter)
        {
            return await _table.Include(x => x.Category)
                               .Include(x => x.Sender)
                               .Include(x => x.Receiver)
                               .Where(filter)
                               .Where(x=>x.IsDraft==false)
                               .OrderByDescending(x=>x.Id)
                               .ToListAsync();

        }

        public async Task<List<Message>> GetAllGarbageBoxAsync(int id)
        {
           
            return await _table.Where(x=>x.ReceiverId==id)
                .Include(x=>x.Sender)
                .Include(x=> x.Receiver)
                .Include(x=>x.Category)
                .Where(x=>x.IsDeleted==true)
                .Where(x=>x.IsDraft==false)
                .Where(x=>x.IsSpam==false)
                .AsNoTracking().ToListAsync();

        }

        public async Task<List<Message>> GetAllSpamAsync(int id)
        {
            return await _table.Include(x => x.Category)
                                 .Include(x => x.Sender)
                                 .Include(x => x.Receiver)
                                 .Where(x=>x.ReceiverId==id)
                                 .Where(x => x.IsDraft == false)
                                 .Where(x => x.IsDeleted == false)
                                 .Where(x => x.IsSpam == true)
                                 .OrderByDescending(x => x.Id)
                                 .ToListAsync();
        }

        public async Task<Message> GetByIdMessageForDraftAsync(int id)
        {
            return await _table.Include(x => x.Receiver).FirstOrDefaultAsync(x => x.Id == id);

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
             var count= await _table.Where(x=>x.ReceiverId == id && x.IsRead == false &&x.IsDeleted==false &&x.IsDraft==false &&x.IsSpam==false).CountAsync();
            
             return count;



        }

        public async Task<List<Message>> GetLast5DontReadMessageAsync(int id)
        {

            var messages = await _table.Where(x => x.ReceiverId == id && x.IsRead == false &&x.IsDraft==false)
                .Include(x=>x.Sender)
                .OrderByDescending(x=>x.Id).Take(5).ToListAsync();

            return messages;

        }

        public async Task<List<Message>> GetMessagesByCategoryId(int id, int userId)
        {
            

            var messages = await _table.Where(x=>x.CategoryId==id).Where(x=>x.IsSpam==false).Where(x=>x.ReceiverId==userId).Include(x=>x.Sender).Include(x=>x.Receiver).
                Include(x=>x.Category).Where(x=>x.IsDeleted==false).ToListAsync();


            return messages;


        }

        public async Task<Message> GetMessageUserSendDateAsync(int id)
        {
            

            var message = await _table.Where(x=>x.SenderId==id).Include(x=>x.Sender).OrderByDescending(x=>x.Id).FirstOrDefaultAsync();
            return message;




        }

        public async Task<List<Message>> PeopleISentMessagesTo(int id)
        {
            var messages = await _table.Where(x=>x.SenderId ==id).Include(x=>x.Receiver).Include(x=>x.Sender).AsNoTracking().ToListAsync();
            var uniqueMessages = messages.DistinctBy(x => x.ReceiverId).ToList();

            return uniqueMessages;

        }
    }
}
