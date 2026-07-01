using RabbitMq.Domain.Entity;

namespace RabbitMQ.Application.Interfaces.Repositories.Orders
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetAllAsync(CancellationToken ct = default);
        Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct = default);
        void CreateOrder(Order order);
        Task UpdateOrderAsync(Order order, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
