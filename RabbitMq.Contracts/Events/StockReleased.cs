namespace RabbitMq.Contracts.Events
{
    public record StockReleased(Guid CorrelationId, Guid OrderId);
}
