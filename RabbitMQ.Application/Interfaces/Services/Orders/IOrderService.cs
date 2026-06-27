using RabbitMq.Domain.Entity;
using RabbitMQ.Application.DTOs;

namespace RabbitMQ.Application.Interfaces.Services.Orders
{
    public interface IOrderService
    {
        Task<List<Order>> GetAllAsync(CancellationToken ct = default);
        Task<Order?> GetByIdAsync(Guid orderId,CancellationToken ct = default);
        Task<Guid> SubmitOrderAsync(CreateOrderRequest request, CancellationToken ct = default);
    }
}
