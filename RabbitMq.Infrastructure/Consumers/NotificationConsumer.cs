using MassTransit;
using Microsoft.Extensions.Logging;
using RabbitMq.Contracts.Events;

namespace RabbitMq.Infrastructure.Messaging.Consumers
{
    // consumer gets events and acts based on what type it is, added as seperate AddConsumer in masstransit
    public class NotificationConsumer : IConsumer<OrderCompleted>, IConsumer<OrderCancelled>
    {
        private readonly ILogger<NotificationConsumer> _logger;

        public NotificationConsumer(ILogger<NotificationConsumer> logger)
        {
            _logger = logger;
        }
        public Task Consume(ConsumeContext<OrderCompleted> context)
        {
            _logger.LogInformation
            (
                "Order {OrderId} completed, notify customer {Email}",
                context.Message.OrderId,
                context.Message.CustomerEmail
            );

            return Task.CompletedTask;
        }
        public Task Consume(ConsumeContext<OrderCancelled> context)
        {
            _logger.LogInformation
            (
                "Order {OrderId} cancelled — notify customer {Email}",
                context.Message.OrderId,
                context.Message.CustomerEmail
            );

            return Task.CompletedTask;
        }

        
    }
}
