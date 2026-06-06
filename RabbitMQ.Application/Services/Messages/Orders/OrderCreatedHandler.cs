using Microsoft.Extensions.Logging;
using RabbitMQ.Application.Models;
using RabbitMQ.Application.Services.Interfaces;
using RabbitMQ.Application.Services.Interfaces.Messages;
using System.Text.Json;

namespace RabbitMQ.Application.Services.Messages.Orders
{
    public class OrderCreatedHandler : IMessageHandler
    {
        private readonly IOrderCreateProcessor _orderProcessor;
        private readonly ILogger<OrderCreatedHandler> _logger;

        public OrderCreatedHandler(IOrderCreateProcessor orderProcessor, ILogger<OrderCreatedHandler> logger)
        {
            _orderProcessor = orderProcessor;
            _logger = logger;
        }

        public async Task HandleAsync(JsonElement payload)
        {
            var order = payload.Deserialize<OrderMessage>() ?? throw new InvalidOperationException("Invalid OrderMessage payload");

            _logger.LogInformation("processing order {Order}", order);
            await _orderProcessor.ProcessOrderAsync(order);
        }
    }
}
