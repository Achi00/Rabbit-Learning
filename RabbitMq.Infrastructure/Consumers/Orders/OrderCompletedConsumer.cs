using MassTransit;
using RabbitMq.Contracts.Commands;
using RabbitMq.Contracts.Events;
using RabbitMQ.Application.Interfaces.Services.Orders;

namespace RabbitMq.Infrastructure.Consumers.Orders
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
            await _orderService.MarkCompletedAsync(context.Message.OrderId, context.CancellationToken);
        }
    }
}
