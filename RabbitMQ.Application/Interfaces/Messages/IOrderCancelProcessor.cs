using RabbitMQ.Application.Models;

namespace RabbitMQ.Application.Interfaces.Messages
{
    public interface IOrderCancelProcessor
    {
        Task CancelOrderAsync(OrderMessage? order);
    }
}
