using Core_IyzicoPaymentSystem.Entities;
using Microsoft.EntityFrameworkCore;
using NotikaEmail_Identity.Context;

namespace Core_IyzicoPaymentSystem.Repositories.OrderRepositories
{
    public class OrderRepository(AppDbContext _context) : IOrderRepository
    {


        public async Task<bool> CreateAsync(Order order)
        {
           await _context.Orders.AddAsync(order);
           var result = await _context.SaveChangesAsync();
            return result > 0;

        }

        public async Task<Order> GetByOrderNoAsync(string orderNo)
        {
            return await _context.Orders.FirstOrDefaultAsync(x => x.OrderNo == orderNo);
        }

        public async Task<bool> UpdateAsync(Order order)
        {
             _context.Orders.Update(order);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}
