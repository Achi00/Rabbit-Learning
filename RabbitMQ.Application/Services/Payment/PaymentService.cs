using RabbitMQ.Application.Interfaces.Repositories.Orders;
using RabbitMQ.Application.Interfaces.Services.Payment;
using RabbitMQ.Application.Results;
using RabbitMq.Domain.Enums;

namespace RabbitMQ.Application.Services.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly IOrderRepository _orderRepository;

        public PaymentService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        public async Task<OperationResult> ChargeAsync(Guid orderId, decimal amount, string customerEmail, CancellationToken ct = default)
        {
            var order = await _orderRepository.GetByIdAsync(orderId, ct);

            if (order is null)
            {
                return OperationResult.Fail("Order not found");
            }

            order.Status = OrderStatus.PaymentCharging;

            var success = Random.Shared.Next(1, 11) <= 8;

            if (!success)
            {
                order.FailureReason = "Payment failed";
                await _orderRepository.SaveChangesAsync(ct);

                return OperationResult.Fail("Payment failed");
            }

            await _orderRepository.SaveChangesAsync(ct);

            return OperationResult.Ok();
        }
    }
}
