using RabbitMq.Domain.Entity;

namespace RabbitMQ.Application.Interfaces.Messages
{
    public interface IOrderCancelProcessor
    {
        Task CancelOrderAsync(Order? order);
    }
}
