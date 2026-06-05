using System.Text.Json;

namespace RabbitMQ.Application.Infrastructure
{
    public static class MessageSerializer
    {
        private static readonly JsonSerializerOptions _options = new() 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        };

        public static byte[] Serialize<T>(T message)
        {
            return JsonSerializer.SerializeToUtf8Bytes(message, _options);
        }
        // avoids string allocation, goes straight from object to  byte[]
        public static T Deserialize<T>(byte[] bytes)
        {
            return JsonSerializer.Deserialize<T>(bytes, _options) ?? throw new InvalidOperationException($"Deserialized {typeof(T).Name} was null");
        }
    }
}
