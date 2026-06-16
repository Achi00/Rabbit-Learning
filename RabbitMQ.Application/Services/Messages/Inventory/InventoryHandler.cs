using RabbitMQ.Application.Services.Interfaces.Messages;
using RabbitMQ.Application.Services.Messages.Idempotency;
using RabbitMqDemo.Persistance.Context;
using System.Text.Json;

namespace RabbitMQ.Application.Services.Messages.Inventory
{
    public class InventoryHandler : IMessageHandler
    {
        private readonly Random _random = new Random();
        private readonly MessageDbContext _context;
        private readonly DbIdempotencyService _idempotency;

        public async Task HandleAsync(JsonElement payload, Guid messageId)
        {
            if (await _idempotency.IsDuplicateAsync(messageId))
            {
                return;
            }

            //var command = payload
        }
    }
}
