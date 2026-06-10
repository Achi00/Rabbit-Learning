using RabbitMq.Domain.Entity;

namespace RabbitMQ.Application.Services.Interfaces
{
    public interface IOrderCreateProcessor
    {
        Task ProcessOrderAsync(Order order);
    }
}
