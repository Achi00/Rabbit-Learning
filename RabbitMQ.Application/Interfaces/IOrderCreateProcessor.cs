using RabbitMQ.Application.Models;

namespace RabbitMQ.Application.Services.Interfaces
{
    public interface IOrderCreateProcessor
    {
        Task ProcessOrderAsync(OrderMessage order);
    }
}
