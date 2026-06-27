using RabbitMq.Domain.Enums;
using RabbitMQ.Application.Interfaces.Repositories.Orders;
using RabbitMQ.Application.Interfaces.Services.Inventory;
using RabbitMQ.Application.Results;

namespace RabbitMQ.Application.Services.Inventory
{
    public class InventoryService : IInventoryService
    {
        private readonly IOrderRepository _orderRepository;

        public InventoryService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        public async Task<OperationResult> ReserveStockAsync(Guid orderId, CancellationToken ct = default)
        {
            var order = await _orderRepository.GetByIdAsync(orderId, ct);

            if (order == null)
            {
                return OperationResult.Fail("Order not found");
            }

            // updatind already exsiting order status
            order.Status = OrderStatus.StockReserving;

            // fake data for learning/testing
            var success = Random.Shared.Next(1, 101) <= 80;

            if (!success)
            {
                order.Status = OrderStatus.Cancelled;
                order.FailureReason = "Not enough stock";

                await _orderRepository.SaveChangesAsync(ct);

                return OperationResult.Fail("Not enough stock");
            }

            await _orderRepository.SaveChangesAsync(ct);

            return OperationResult.Ok();
        }

        public async Task<OperationResult> ReleaseStockAsync(Guid orderId, CancellationToken ct = default)
        {
            var order = await _orderRepository.GetByIdAsync(orderId, ct);

            if (order is null)
            {
                return OperationResult.Fail("Order not found");
            }

            order.Status = OrderStatus.Compensating;

            await _orderRepository.SaveChangesAsync(ct);

            return OperationResult.Ok();
        }
    }
}
