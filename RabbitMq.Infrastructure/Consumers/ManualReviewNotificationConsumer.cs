using MassTransit;
using RabbitMq.Contracts.Commands;

namespace RabbitMq.Infrastructure.Consumers
{
    public class ManualReviewNotificationConsumer : IConsumer<OrderRequiresManualReview>
    {
        // log for testing, later maybe will email to admin, create ticker or write it in db
        public Task Consume(ConsumeContext<OrderRequiresManualReview> context)
        {
            Console.WriteLine($"Order {context.Message.OrderId} requires manual review: {context.Message.FailureReason}"));

            return Task.CompletedTask;
        }
    }
}
