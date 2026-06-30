namespace RabbitMq.Contracts.Events
{
    public record StockReserved(Guid CorrelationId, Guid OrderId);

}
