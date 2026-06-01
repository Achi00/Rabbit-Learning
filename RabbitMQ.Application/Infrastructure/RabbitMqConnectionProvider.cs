using RabbitMQ.Client;

namespace RabbitMQ.Application.Infrastructure
{
    // singleton connection provider IConnection
    public class RabbitMqConnectionProvider : IAsyncDisposable
    {
        private readonly IConnection _connection;

        public RabbitMqConnectionProvider(IConnection connection)
        {
            _connection = connection;
        }

        public IConnection Connection => _connection;

        public static async Task<RabbitMqConnectionProvider> CreateAsync(string hostName, CancellationToken ct = default)
        {
            var factory = new ConnectionFactory { HostName = hostName };
            var connection = await factory.CreateConnectionAsync(ct);

            return new RabbitMqConnectionProvider(connection);
        }

        public async ValueTask DisposeAsync()
        {
            await _connection.DisposeAsync();
        }
    }
}
