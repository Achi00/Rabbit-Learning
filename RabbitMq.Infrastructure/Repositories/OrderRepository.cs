using RabbitMq.Domain.Entity;
using RabbitMQ.Application.Interfaces.Repositories.Orders;
using RabbitMqDemo.Persistance.Context;

namespace RabbitMq.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly MessageDbContext _context;

        public void CreateOrder(Order order, CancellationToken ct = default)
        {
            _context.Orders.Add(order);
        }

        public Task<List<Order>> GetAllAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<Order> GetByIdAsync(Guid orderId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public void UpdateOrder(Order order, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
