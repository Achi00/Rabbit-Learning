namespace RabbitMq.Contracts.Commands
{
    // compensation
    public record ReleaseStockCommand(Guid SagaId, Guid OrderId);
}
