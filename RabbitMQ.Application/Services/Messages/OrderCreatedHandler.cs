using RabbitMQ.Application.Models;
using RabbitMQ.Application.Services.Interfaces;
using RabbitMQ.Application.Services.Interfaces.Messages;
using System.Text.Json;

namespace RabbitMQ.Application.Services.Messages
{
    public class OrderCreatedHandler : IMessageHandler
    {
        private readonly IOrderProcessor _orderProcessor;

        public OrderCreatedHandler(IOrderProcessor orderProcessor)
        {
            _orderProcessor = orderProcessor;
        }

        public async Task HandleAsync(JsonElement payload)
        {
            var order = payload.Deserialize<OrderMessage>() ?? throw new InvalidOperationException("Invalid OrderMessage payload");

            await _orderProcessor.ProcessOrderAsync(order);
        }
    }
}
