using RabbitMQ.Application.Models;

namespace RabbitMQ.Application.Services.Interfaces
{
    public interface IOrderProcessor
    {
        Task ProcessOrderAsync(OrderMessage order);
    }
}
