namespace RabbitMq.Contracts.Events
{
    public record StockReleaseFailed(Guid SagaId, Guid OrderId, string Reason);
}
