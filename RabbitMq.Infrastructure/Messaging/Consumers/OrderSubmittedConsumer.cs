using MassTransit;
using RabbitMq.Contracts.Events;

namespace RabbitMq.Infrastructure.Messaging.Consumers
{
    public class OrderSubmittedConsumer : IConsumer<OrderSubmittedEvent>
    {
        public async Task Consume(ConsumeContext<OrderSubmittedEvent> context)
        {
            // saga handles OrderSubmitted directly
        }
    }
}
