using MassTransit;
using RabbitMq.Contracts.Commands;
using RabbitMq.Contracts.Events;
using RabbitMQ.Application.Interfaces.Services.Inventory;

namespace RabbitMq.Infrastructure.Messaging.Consumers
{
    public class ReleaseStockConsumer : IConsumer<ReleaseStock>
    {
        private readonly IInventoryService _inventoryService;
        public ReleaseStockConsumer(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }
        public async Task Consume(ConsumeContext<ReleaseStock> context)
        {
            var result = await _inventoryService.ReleaseStockAsync(context.Message.OrderId);

            if (result.Success)
            {
                await context.Publish(new StockReleased(context.Message.SagaId, context.Message.OrderId));
            }
            else
            {
                await context.Publish(new StockReleaseFailed(context.Message.SagaId, context.Message.OrderId, result.ErrorMessage ?? "Stock release failed"));
            }
        }
    }
}
