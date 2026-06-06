using System.Text.Json;

namespace RabbitMQ.Application.Services.Interfaces.Messages
{
    public interface IMessageHandler
    {
        Task HandleAsync(JsonElement payload);
    }
}
