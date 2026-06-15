namespace RabbitMq.Contracts.Commands
{
    public record ChargePaymentCommand(Guid SagaId, Guid OrderId, decimal Amount, string CustomerEmail);
}
