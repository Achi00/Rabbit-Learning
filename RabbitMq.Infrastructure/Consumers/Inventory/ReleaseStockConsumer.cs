using MassTransit;
using Microsoft.EntityFrameworkCore;
using RabbitMq.Contracts.Commands;
using RabbitMq.Contracts.Events;
using RabbitMQ.Application.Interfaces.Services.Inventory;
using RabbitMqDemo.Persistance.Context;

namespace RabbitMq.Infrastructure.Messaging.Consumers
{
    public class ReleaseStockConsumer : IConsumer<ReleaseStock>
    {
        private readonly IInventoryService _inventoryService;
        private readonly MessageDbContext _context;
        public ReleaseStockConsumer(IInventoryService inventoryService, MessageDbContext context)
        {
            _inventoryService = inventoryService;
            _context = context;
        }
        public async Task Consume(ConsumeContext<ReleaseStock> context)
        {
            var messageId = context.MessageId;
            var alreadyProcessed = await _context.Messages.AnyAsync(m => m.MessageId == messageId);

            if (alreadyProcessed)
            {
                return;
            }

            var result = await _inventoryService.ReleaseStockAsync(context.Message.OrderId, context.CancellationToken);

            if (result.Success)
            {
                await context.Publish(new StockReleased(context.Message.CorrelationId, context.Message.OrderId));
            }
            else
            {
                await context.Publish(new StockReleaseFailed(context.Message.CorrelationId, context.Message.OrderId, result.ErrorMessage ?? "Stock release failed"));
            }
        }
    }
}
