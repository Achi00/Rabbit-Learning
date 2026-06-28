using MassTransit;
using RabbitMq.Contracts.Commands;
using RabbitMq.Contracts.Events;

namespace RabbitMq.Infrastructure.Consumers.Payment
{
    public class ChargePaymentConsumer : IConsumer<ChargePayment>
    {
        public async Task Consume(ConsumeContext<ChargePayment> context)
        {
            var success = Random.Shared.Next(10) > 3;

            if (success)
            {
                await context.Publish(new PaymentCharged(context.Message.SagaId, context.Message.OrderId));
            }
            else
            {
                await context.Publish(new PaymentFailed(context.Message.SagaId, context.Message.OrderId, "Card Declined"));
            }
        }
    }
}
