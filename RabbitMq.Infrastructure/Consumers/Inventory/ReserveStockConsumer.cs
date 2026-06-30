using MassTransit;
using RabbitMq.Contracts.Commands;
using RabbitMq.Contracts.Events;
using RabbitMQ.Application.Interfaces.Services.Inventory;

namespace RabbitMq.Infrastructure.Consumers.Inventory
{
    // consumer gets involved when pirst message is already published

    public class ReserveStockConsumer : IConsumer<ReserveStock>
    {
        private readonly IInventoryService _inventoryService;

        public ReserveStockConsumer(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }
        public async Task Consume(ConsumeContext<ReserveStock> context)
        {
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
