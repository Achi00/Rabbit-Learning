using RabbitMQ.Application.DTOs;

namespace RabbitMQ.Application.Interfaces.Services.Orders
{
    public interface IOrderService
    {
        Task SubmitOrderAsync(CreateOrderRequest dto);
    }
}
