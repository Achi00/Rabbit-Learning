using RabbitMQ.Application.Services.Interfaces.Messages;
using RabbitMqDemo.Persistance.Context;
using System.Text.Json;

namespace RabbitMQ.Application.Services.Messages.Inventory
{
    public class InventoryHandler : IMessageHandler
    {
        private readonly Random _random = new Random();
        private readonly MessageDbContext _context;

        public async Task HandleAsync(JsonElement payloadm, Guid messageId)
        {
            //var idempotancy = await _
        }
    }
}
