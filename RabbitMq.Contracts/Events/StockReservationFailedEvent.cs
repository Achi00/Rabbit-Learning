namespace RabbitMq.Contracts.Events
{
    public record StockReservationFailedEvent(Guid SagaId, Guid OrderId, string Reason);

}
