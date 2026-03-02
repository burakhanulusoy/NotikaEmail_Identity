using Core_IyzicoPaymentSystem.Entities;

namespace Core_IyzicoPaymentSystem.Repositories.OrderRepositories
{
    public interface IOrderRepository
    {
        Task<bool> CreateAsync(Order order);
        Task<bool> UpdateAsync(Order order);
        Task<Order> GetByOrderNoAsync(string orderNo);
       

    }
}
