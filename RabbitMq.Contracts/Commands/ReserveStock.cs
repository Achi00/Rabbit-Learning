namespace RabbitMq.Contracts.Commands
{
    public record ReserveStock(Guid CorrelationId, Guid OrderId);
}
