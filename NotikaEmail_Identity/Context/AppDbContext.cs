using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NotikaEmail_Identity.Entities;

namespace NotikaEmail_Identity.Context
{
    public class AppDbContext:IdentityDbContext<AppUser,AppRole,int>
    {

        public AppDbContext(DbContextOptions options):base(options)
        {
            

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {

            // Yapma nedenim 
            //
            //
            //
            //
            //
            //
            //
            //
            //


            base.OnModelCreating(builder);
            builder.Entity<AppUser>()
                .HasMany(user => user.SentMessages)  // 1. Bir AppUser'ın BİRDEN FAZLA gönderdiği mesajı (SentMessages) olabilir.
                .WithOne(x => x.Sender)              // 2. Bu mesajların her birinin SADECE BİR göndericisi (Sender) vardır.
                .HasForeignKey(x => x.SenderId)      // 3. Bu ilişkiyi veritabanında 'SenderId' kolonu üzerinden birbirine bağla.
                .OnDelete(DeleteBehavior.Restrict);  // 4. (Çok Önemli) Kullanıcı silinirse mesajlarını otomatik SİLME!
         
            
            builder.Entity<AppUser>().HasMany(message => message.ReceivedMessages)
           .WithOne(x => x.Receiver)
           .HasForeignKey(x => x.ReceiverId).OnDelete(DeleteBehavior.Restrict);



            
        }

        public DbSet<Message> Messages { get; set; }
        public DbSet<Category> Categories { get; set; }




    }
}
