using MassTransit;
using RabbitMq.Contracts.Commands;
using RabbitMq.Contracts.Events;

namespace RabbitMq.Infrastructure.Messaging.Consumers
{
    public class ReleaseStockConsumer : IConsumer<ReleaseStock>
    {
        public async Task Consume(ConsumeContext<ReleaseStock> context)
        {
            await context.Publish(new StockReleased(context.Message.SagaId, context.Message.OrderId));
        }
    }
}
