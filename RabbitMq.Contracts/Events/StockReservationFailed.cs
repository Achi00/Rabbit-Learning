namespace RabbitMq.Contracts.Events
{
    public record StockReservationFailed(Guid SagaId, Guid OrderId, string Reason);

}
