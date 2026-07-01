using MassTransit;
using Microsoft.EntityFrameworkCore;
using RabbitMq.Contracts.Events;
using RabbitMq.Domain.Enums;
using RabbitMQ.Application.Interfaces.Services.Orders;
using RabbitMqDemo.Persistance.Context;

namespace RabbitMq.Infrastructure.Consumers.Orders
{
    public class OrderCancelledConsumer : IConsumer<OrderCancelled>
    {
        private readonly IOrderService _orderService;
        private readonly MessageDbContext _context;

        public OrderCancelledConsumer(IOrderService orderService, MessageDbContext context)
        {
            _orderService = orderService;
            _context = context;
        }
        public async Task Consume(ConsumeContext<OrderCancelled> context)
        {
            var messageId = context.MessageId;

            // idempotency check manually, try inbox patter if it fixes this????
            var alreadyProcessed = await _context.Messages.AnyAsync(m => m.MessageId == messageId);

            if (alreadyProcessed)
            {
                return;
            }

            await _orderService.MarkCancelledAsync(context.Message.OrderId, context.Message.Reason, context.CancellationToken);
        }
    }
}
