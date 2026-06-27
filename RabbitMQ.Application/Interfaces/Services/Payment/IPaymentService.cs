using RabbitMQ.Application.Results;

namespace RabbitMQ.Application.Interfaces.Services.Payment
{
    public interface IPaymentService
    {
        Task<OperationResult> ChargeAsync(Guid orderId, decimal amount, string customerEmail, CancellationToken ct = default);
    }
}
