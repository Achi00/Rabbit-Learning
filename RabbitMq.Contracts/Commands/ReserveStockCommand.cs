namespace RabbitMq.Contracts.Commands
{
    public record ReserveStockCommand(Guid SagaId, Guid OrderId);
}
