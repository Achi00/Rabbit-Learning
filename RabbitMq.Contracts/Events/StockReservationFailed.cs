namespace RabbitMq.Contracts.Events
{
    public record StockReservationFailed(Guid CorrelationId, Guid OrderId, string Reason);

}
