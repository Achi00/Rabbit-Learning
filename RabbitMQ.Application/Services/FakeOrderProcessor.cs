using RabbitMQ.Application.Exceptions;
using RabbitMQ.Application.Models;
using RabbitMQ.Application.Services.Interfaces;
using System.Text.Json;

namespace RabbitMQ.Application.Services
{
    public sealed class FakeOrderProcessor : IOrderCreateProcessor
    {
        // 70% success 30% failure, to test different acknowledgement patterns
        private readonly Random _random = new Random();
        public async Task ProcessOrderAsync(OrderMessage order)
        {
            if (string.IsNullOrWhiteSpace(order.CustomerEmail))
            {
                throw new InvalidOrderException("Customer email mising");
            }

            if (order.Amount <= 0)
            {
                throw new InvalidOrderException("Order amount must be positive value");
            }
            await Task.Delay(1000);
            // removed to test idempotency
            //var result = _random.Next(1, 10);

            //if (result <= 3)
            //{
            //    throw new HttpRequestException("Payment gateway unavailable - will retry again");
            //}
            //else if (result >= 7)
            //{
            //    throw new InvalidOrderException("Formatting error - will not retry again");
            //}

            Console.WriteLine($"Processed order {order.Id}");
        }
    }
}
