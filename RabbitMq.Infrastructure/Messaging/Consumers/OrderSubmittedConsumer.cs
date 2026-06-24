using MassTransit;
using RabbitMq.Contracts.Events;

namespace RabbitMq.Infrastructure.Messaging.Consumers
{
    public class OrderSubmittedConsumer : IConsumer<OrderSubmitted>
    {
        public async Task Consume(ConsumeContext<OrderSubmitted> context)
        {
            // saga handles OrderSubmitted directly
        }
    }
}
