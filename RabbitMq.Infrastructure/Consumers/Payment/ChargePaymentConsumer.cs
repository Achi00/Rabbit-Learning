using MassTransit;
using Microsoft.EntityFrameworkCore;
using RabbitMq.Contracts.Commands;
using RabbitMq.Contracts.Events;
using RabbitMqDemo.Persistance.Context;

namespace RabbitMq.Infrastructure.Consumers.Payment
{
    public class ChargePaymentConsumer : IConsumer<ChargePayment>
    {
        private readonly MessageDbContext _context;

        public ChargePaymentConsumer(MessageDbContext context)
        {
            _context = context;
        }
        public async Task Consume(ConsumeContext<ChargePayment> context)
        {
            var messageId = context.MessageId;
            // idempotency check manually, try inbox patter if it fixes this????
            var alreadyProcessed = await _context.Messages.AnyAsync(m => m.MessageId == messageId);

            if (alreadyProcessed)
            {
                return;
            }

            var success = Random.Shared.Next(10) > 3;

            if (success)
            {
                await context.Publish(new PaymentCharged(context.Message.CorrelationId, context.Message.OrderId));
            }
            else
            {
                await context.Publish(new PaymentFailed(context.Message.CorrelationId, context.Message.OrderId, "Card Declined"));
            }
        }
    }
}
