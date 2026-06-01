using Microsoft.Extensions.Hosting;
using RabbitMQ.Application.Infrastructure;

namespace RabbitMQ.Application
{
    public class TopologyHostedService : IHostedService
    {
        private readonly TopologySetup _topology;
        public TopologyHostedService(TopologySetup topology)
        {
            _topology = topology;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _topology.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
