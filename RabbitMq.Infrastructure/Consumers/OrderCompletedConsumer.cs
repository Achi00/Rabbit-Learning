using MassTransit;
using RabbitMq.Contracts.Commands;
using RabbitMq.Contracts.Events;
using RabbitMQ.Application.Interfaces.Services.Orders;

namespace RabbitMq.Infrastructure.Consumers
{
    public class OrderCompletedConsumer : IConsumer<OrderCompleted>
    {
        private readonly IOrderService _orderService;

        public OrderCompletedConsumer(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task Consume(ConsumeContext<OrderCompleted> context)
        {
            var order = await _orderService.GetByIdAsync(context.Message.OrderId);

            if (order is not null)
            {
                await context.Publish(new OrderCompleted(context.Message.OrderId, context.Message.CustomerEmail));
            }
            else
            {
                await context.Publish(new OrderCancelled(context.Message.OrderId, context.Message.CustomerEmail));
            }
        }
    }
}
