using MassTransit;
using Microsoft.EntityFrameworkCore;
using RabbitMq.Contracts.Commands;
using RabbitMq.Contracts.Events;
using RabbitMQ.Application.Interfaces.Services.Inventory;
using RabbitMqDemo.Persistance.Context;

namespace RabbitMq.Infrastructure.Consumers.Inventory
{
    // consumer gets involved when pirst message is already published

    public class ReserveStockConsumer : IConsumer<ReserveStock>
    {
        private readonly IInventoryService _inventoryService;
        private readonly MessageDbContext _context;

        public ReserveStockConsumer(IInventoryService inventoryService, MessageDbContext context)
        {
            _inventoryService = inventoryService;
            _context = context;
        }
        public async Task Consume(ConsumeContext<ReserveStock> context)
        {
            var messageId = context.MessageId;
            // idempotency check manually, try inbox patter if it fixes this????
            var alreadyProcessed = await _context.Messages.AnyAsync(m => m.MessageId == messageId);

            if (alreadyProcessed)
            {
                return;
            }
            var message = context.Message;
            // consumer gets only existing message
            var result = await _inventoryService.ReserveStockAsync(message.OrderId, context.CancellationToken);

            if (result.Success)
            {
                await context.Publish(new StockReserved(message.CorrelationId, message.OrderId));
            }
            else
            {
                await context.Publish(new StockReservationFailed(message.CorrelationId, message.OrderId, result.ErrorMessage ?? "Stock reservation failed"));
            }
        }
    }
}
