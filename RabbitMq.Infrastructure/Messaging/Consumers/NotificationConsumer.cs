using MassTransit;
using Microsoft.Extensions.Logging;
using RabbitMq.Contracts.Commands;
using RabbitMq.Contracts.Events;

namespace RabbitMq.Infrastructure.Messaging.Consumers
{
    public class NotificationConsumer : IConsumer<Contracts.Commands.OrderCompleted>, IConsumer<Contracts.Commands.OrderCancelled>
    {
        private readonly ILogger<NotificationConsumer> _logger;

        public NotificationConsumer(ILogger<NotificationConsumer> logger)
        {
            _logger = logger;
        }
        public Task Consume(ConsumeContext<Contracts.Commands.OrderCompleted> context)
        {
            _logger.LogInformation
            (
                "Order {OrderId} completed, notify customer {Email}",
                context.Message.OrderId,
                context.Message.CustomerEmail
            );

            return Task.CompletedTask;
        }
        public Task Consume(ConsumeContext<Contracts.Commands.OrderCancelled> context)
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
