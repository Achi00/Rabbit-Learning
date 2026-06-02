using RabbitMQ.Application.Models;
using RabbitMQ.Application.Services.Interfaces;

namespace RabbitMQ.Application.Services
{
    public sealed class FakeOrderProcessor : IOrderProcessor
    {
        private readonly Random _random = new Random();
        public async Task ProcessOrderAsync(OrderMessage order)
        {
            await Task.Delay(1000);

            var result = _random.Next(1, 10);

            if (result <= 3)
            {
                throw new HttpRequestException("Payment gateway unavailable");
            }

            Console.WriteLine($"Processed order {order.Id}");
        }
    }
}
