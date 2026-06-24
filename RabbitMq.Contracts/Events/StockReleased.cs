namespace RabbitMq.Contracts.Events
{
    public record StockReleased(Guid SagaId, Guid OrderId);
}
