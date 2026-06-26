using MassTransit;
using RabbitMq.Contracts.Commands;
using RabbitMq.Contracts.Events;

namespace RabbitMq.Infrastructure.Messaging.Consumers
{
    // consumer gets involved when pirst message is already published

    public class ReserveStockConsumer : IConsumer<ReserveStock>
    {
        public async Task Consume(ConsumeContext<ReserveStock> context)
        {
            
        }
    }
}
