namespace RabbitMq.Contracts.Events
{
    public record StockReservedEvent(Guid SagaId, Guid OrderId);

}
