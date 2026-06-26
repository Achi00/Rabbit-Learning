using RabbitMQ.Application.Results;

namespace RabbitMQ.Application.Interfaces.Services.Inventory
{
    public interface IInventoryService
    {
        Task<OperationResult> ReserveStockAsync(Guid orderId, CancellationToken ct = default);

        Task<OperationResult> ReleaseStockAsync(Guid orderId, CancellationToken ct = default);
    }
}
