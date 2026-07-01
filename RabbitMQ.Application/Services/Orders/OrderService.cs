using Azure.Core;
using MassTransit;
using RabbitMq.Contracts.Events;
using RabbitMq.Domain.Entity;
using RabbitMq.Domain.Enums;
using RabbitMQ.Application.DTOs;
using RabbitMQ.Application.Interfaces.Repositories.Orders;
using RabbitMQ.Application.Interfaces.Services.Orders;

namespace RabbitMQ.Application.Services.Orders
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderService(IOrderRepository orderRepository, IPublishEndpoint publishEndpoint)
        {
            _orderRepository = orderRepository;
            _publishEndpoint = publishEndpoint;
        }
        public async Task<List<Order>> GetAllAsync(CancellationToken ct = default)
        {
            return await _orderRepository.GetAllAsync(ct);
        }

        public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct = default)
        {
            var order = await _orderRepository.GetByIdAsync(orderId, ct);

            if (order is null)
            {
                throw new Exception("Order was not found");
            }
            return order;
        }

        public async Task MarkCancelledAsync(Guid orderId, string? reason, CancellationToken ct = default)
        {
            var order = await _orderRepository.GetByIdAsync(orderId, ct);

            if (order is null)
            {
                return;
            }
            // will avoid idempotency issue
            if (order.Status == OrderStatus.Cancelled)
            {
                return;
            }
            order.Status = OrderStatus.Cancelled;
            order.CompletedAt = DateTimeOffset.UtcNow;
            order.FailureReason = reason;

            _orderRepository.UpdateOrderAsync(order, ct);

            await _orderRepository.SaveChangesAsync(ct);

        }

        public async Task MarkCompletedAsync(Guid orderId, CancellationToken ct = default)
        {
            var order = await _orderRepository.GetByIdAsync(orderId, ct);

            if (order is null)
            {
                return;
            }

            // will avoid idempotency issue
            if (order.Status == OrderStatus.Completed)
            {
                return;
            }

            order.Status = OrderStatus.Completed;
            order.CompletedAt = DateTimeOffset.UtcNow;
            order.FailureReason = null;

            _orderRepository.UpdateOrderAsync(order, ct);

            await _orderRepository.SaveChangesAsync(ct);
        }

        // should include unit of works
        public async Task<Guid> SubmitOrderAsync(CreateOrderRequest request, CancellationToken ct = default)
        {
            var orderId = Guid.NewGuid();

            var order = new Order
            {
                Id = orderId,
                CustomerEmail = request.CustomerEmail,
                Amount = request.Amount,
                Status = OrderStatus.Submitted,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _orderRepository.CreateOrder(order, ct);

            // first publishe then save changes, because we use outbox pattern
            await _publishEndpoint.Publish(new OrderSubmitted(orderId, order.CustomerEmail, order.Amount), ct);

            await _orderRepository.SaveChangesAsync(ct);

            return orderId;
        }
    }
}
