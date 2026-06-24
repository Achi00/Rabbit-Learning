namespace RabbitMq.Contracts.Commands
{
    public record ReserveStock(Guid SagaId, Guid OrderId);
}
