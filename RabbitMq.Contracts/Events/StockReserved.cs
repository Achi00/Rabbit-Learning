namespace RabbitMq.Contracts.Events
{
    public record StockReserved(Guid SagaId, Guid OrderId);

}
