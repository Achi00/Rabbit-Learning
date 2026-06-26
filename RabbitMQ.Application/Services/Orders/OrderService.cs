using RabbitMQ.Application.DTOs;
using RabbitMQ.Application.Interfaces.Services.Orders;

namespace RabbitMQ.Application.Services.Orders
{
    public class OrderService : IOrderService
    {
        public Task SubmitOrderAsync(CreateOrderRequest dto)
        {
            throw new NotImplementedException();
        }
    }
}
