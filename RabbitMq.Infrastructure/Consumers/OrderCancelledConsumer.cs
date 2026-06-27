using MassTransit;
using RabbitMq.Contracts.Events;
using RabbitMq.Domain.Enums;
using RabbitMQ.Application.Interfaces.Services.Orders;

namespace RabbitMq.Infrastructure.Consumers
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
            var order = await _orderService.GetByIdAsync(context.Message.OrderId);
            if (order is null)
            {
                await context.Publish(new OrderCancelled(context.Message.OrderId, context.Message.CustomerEmail));
            }
            else if (order.Status == OrderStatus.Cancelled)
            {
                await context.Publish(new OrderCancelled(context.Message.OrderId, context.Message.CustomerEmail));
            }
        }
    }
}
