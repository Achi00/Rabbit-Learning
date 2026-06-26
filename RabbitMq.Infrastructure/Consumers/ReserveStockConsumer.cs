using MassTransit;
using RabbitMq.Contracts.Commands;
using RabbitMq.Contracts.Events;

namespace RabbitMq.Infrastructure.Messaging.Consumers
{
    public class ReserveStockConsumer : IConsumer<ReserveStock>
    {
        public async Task Consume(ConsumeContext<ReserveStock> context)
        {
            // randomize succes for testing
            var success = Random.Shared.Next(10) > 3;

            if (success)
            {
                // published event back to state machine, it should updates state based on type
                await context.Publish
                (
                    new StockReserved(context.Message.SagaId, context.Message.OrderId)
                );
            }
            else
            {
                await context.Publish
                (
                    new StockReservationFailed(context.Message.SagaId, context.Message.OrderId, "Out of stock")
                );
            }
        }
    }
}
