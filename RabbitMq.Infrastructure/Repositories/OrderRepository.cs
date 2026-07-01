using Microsoft.EntityFrameworkCore;
using RabbitMq.Domain.Entity;
using RabbitMQ.Application.Interfaces.Repositories.Orders;
using RabbitMqDemo.Persistance.Context;

namespace RabbitMq.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly MessageDbContext _context;

        public OrderRepository(MessageDbContext context)
        {
            _context = context;
        }

        public void CreateOrder(Order order)
        {
            _context.Orders.Add(order);
        }

        public async Task<List<Order>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Orders.AsNoTracking().ToListAsync(ct);
        }

        public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == orderId, ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateOrderAsync(Order order, CancellationToken ct = default)
        {
            var exists = await _context.Orders.FirstOrDefaultAsync(o => o.Id == order.Id, ct);

            if (exists is null)
            {
                // this should be custom not found exception, but in this case fine...
                throw new Exception("Order was not fount");
            }
            // this should be passed dto for updating but in this case also fine..
            // update only amount
            //exists = exists with { Amount = order.Amount };
        }
    }
}
