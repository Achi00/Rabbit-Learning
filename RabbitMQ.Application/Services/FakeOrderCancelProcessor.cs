using RabbitMq.Domain.Entity;
using RabbitMQ.Application.Exceptions;
using RabbitMQ.Application.Interfaces.Messages;

namespace RabbitMQ.Application.Services
{
    public class FakeOrderCancelProcessor : IOrderCancelProcessor
    {
        private readonly Random _random = new Random();
        public async Task CancelOrderAsync(Order? order)
        {
            if (order == null)
            {
                throw new InvalidOrderException("Order cant be found");
            }
            if (order.Amount <= 0)
            {
                throw new InvalidOrderException("Cannot cancel order with invalid amount");
            }

            // testing with random values
            await Task.Delay(500);

            if (_random.Next(1, 10) <= 6)
            {
                throw new HttpRequestException("Refund service unavailable");
            }
        }
    }
}
