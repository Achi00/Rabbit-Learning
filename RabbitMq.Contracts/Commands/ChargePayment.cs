namespace RabbitMq.Contracts.Commands
{
    public record ChargePayment(Guid SagaId, Guid OrderId, decimal Amount, string CustomerEmail);
}
