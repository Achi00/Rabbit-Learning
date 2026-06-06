using Microsoft.Extensions.Logging;
using RabbitMQ.Application.Interfaces.Messages;
using RabbitMQ.Application.Models;
using RabbitMQ.Application.Services.Interfaces;
using RabbitMQ.Application.Services.Interfaces.Messages;
using System.Text.Json;

namespace RabbitMQ.Application.Services.Messages.Orders
{
    public class OrderCancelledHandler : IMessageHandler
    {
        private readonly IOrderCancelProcessor _orderCancelProcessor;
        private readonly ILogger<OrderCancelledHandler> _logger;

        public OrderCancelledHandler(IOrderCancelProcessor orderCancelProcessor, ILogger<OrderCancelledHandler> logger)
        {
            _orderCancelProcessor = orderCancelProcessor;
            _logger = logger;
        }
        public async Task HandleAsync(JsonElement payload)
        {
            var order = payload.Deserialize<OrderMessage>();

            _logger.LogInformation("Cancelling order {Order}", order);
            await _orderCancelProcessor.CancelOrderAsync(order);
        }
    }
}
