namespace RabbitMq.Contracts.Commands
{
    // compensation
    public record ReleaseStock(Guid CorrelationId, Guid OrderId);
}
