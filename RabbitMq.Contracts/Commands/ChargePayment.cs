namespace RabbitMq.Contracts.Commands
{
    public record ChargePayment(Guid CorrelationId, Guid OrderId, decimal Amount, string CustomerEmail);
}
