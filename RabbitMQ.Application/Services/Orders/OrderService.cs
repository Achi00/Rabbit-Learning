using Azure.Core;
using RabbitMq.Domain.Entity;
using RabbitMQ.Application.DTOs;
using RabbitMQ.Application.Interfaces.Repositories.Orders;
using RabbitMQ.Application.Interfaces.Services.Orders;

namespace RabbitMQ.Application.Services.Orders
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        public async Task<List<Order>> GetAllAsync(CancellationToken ct = default)
        {
            return await _orderRepository.GetAllAsync(ct);
        }

        // should include unit of works
        public Task SubmitOrderAsync(CreateOrderRequest request, CancellationToken ct = default)
        {
            var orderId = Guid.NewGuid();

            var order = new Order
            {
                Id = orderId,
                ConsumerEmail = request.ConsumerEmail,
                Amount = request.Amount,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _orderRepository.CreateOrder(order, ct);

            return Task.CompletedTask;
        }
    }
}
