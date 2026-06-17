namespace RabbitMq.Contracts.Events
{
    public record StockReleasedEvent(Guid SagaId, Guid OrderId);
}
