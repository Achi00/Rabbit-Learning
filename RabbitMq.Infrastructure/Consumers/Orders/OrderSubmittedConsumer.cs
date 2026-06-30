using MassTransit;
using RabbitMq.Contracts.Events;

namespace RabbitMq.Infrastructure.Consumers.Orders
{
    public class OrderSubmittedConsumer : IConsumer<OrderSubmitted>
    {
        public async Task Consume(ConsumeContext<OrderSubmitted> context)
        {
            await context.Publish(new OrderSubmitted(context.Message.OrderId, context.Message.CustomerEmail, context.Message.Amount));
        }
    }
}
