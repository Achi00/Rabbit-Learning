using MassTransit;
using Microsoft.EntityFrameworkCore;
using RabbitMq.Contracts.Commands;
using RabbitMq.Contracts.Events;
using RabbitMQ.Application.Interfaces.Services.Orders;
using RabbitMqDemo.Persistance.Context;

namespace RabbitMq.Infrastructure.Consumers.Orders
{
    public class OrderCompletedConsumer : IConsumer<OrderCompleted>
    {
        private readonly IOrderService _orderService;
        private readonly MessageDbContext _context;

        public OrderCompletedConsumer(IOrderService orderService, MessageDbContext context)
        {
            _orderService = orderService;
            _context = context;
        }

        public async Task Consume(ConsumeContext<OrderCompleted> context)
        {
            var messageId = context.MessageId;
            // idempotency check manually, try inbox patter if it fixes this????
            var alreadyProcessed = await _context.Messages.AnyAsync(m => m.MessageId == messageId);

            if (alreadyProcessed)
            {
                return;
            }
            await _orderService.MarkCompletedAsync(context.Message.OrderId, context.CancellationToken);
        }
    }
}
