using RabbitMQ.Client;

namespace RabbitMQ.Application.Infrastructure
{
    // singleton connection provider IConnection
    public class RabbitMqConnectionProvider : IAsyncDisposable
    {
        private IConnection? _connection;
        private readonly ConnectionFactory _factory;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public RabbitMqConnectionProvider(string hostname)
        {
            // configure factory, connection only should connect on first use rather than construct time
            // otherwise it caused hanging service when rabbitmq was not running, beats purpouse of decoupling, outbox pattern
            _factory = new ConnectionFactory
            {
                HostName = hostname,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
            };
        }

        //public IConnection Connection => _connection;

        public async Task<IConnection> GetConnetionAsync()
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            await _lock.WaitAsync();

            try
            {
                if (_connection is { IsOpen: true })
                {
                    return _connection;
                }

                _connection = await _factory.CreateConnectionAsync();
                return _connection;
            }
            finally
            {
                _lock.Release();
            }
        }

        //public static async Task<RabbitMqConnectionProvider> CreateAsync(string hostName, CancellationToken ct = default)
        //{
            

        //    var connection = await factory.CreateConnectionAsync(ct);

        //    return new RabbitMqConnectionProvider(connection);
        //}

        public async ValueTask DisposeAsync()
        {
            if (_connection != null)
            {
                await _connection.DisposeAsync();
            }
        }
    }
}
