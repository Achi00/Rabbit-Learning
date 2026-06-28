namespace RabbitMq.Contracts.Events
{
    public record CompensationFailed(Guid OrderId, string Reason);
}
