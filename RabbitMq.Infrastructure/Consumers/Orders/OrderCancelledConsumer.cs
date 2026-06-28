using MassTransit;
using RabbitMq.Contracts.Events;
using RabbitMq.Domain.Enums;
using RabbitMQ.Application.Interfaces.Services.Orders;

namespace RabbitMq.Infrastructure.Consumers.Orders
{
    public class OrderCancelledConsumer : IConsumer<OrderCancelled>
    {
        private readonly IOrderService _orderService;

        public OrderCancelledConsumer(IOrderService orderService)
        {
            _orderService = orderService;
        }
        public async Task Consume(ConsumeContext<OrderCancelled> context)
        {
            await _orderService.MarkCancelledAsync(context.Message.OrderId, context.Message.Reason, context.CancellationToken);
        }
    }
}
