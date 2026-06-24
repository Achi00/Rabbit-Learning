namespace RabbitMq.Contracts.Commands
{
    // compensation
    public record ReleaseStock(Guid SagaId, Guid OrderId);
}
