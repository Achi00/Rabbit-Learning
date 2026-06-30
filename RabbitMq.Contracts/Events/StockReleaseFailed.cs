namespace RabbitMq.Contracts.Events
{
    public record StockReleaseFailed(Guid CorrelationId, Guid OrderId, string Reason);
}
