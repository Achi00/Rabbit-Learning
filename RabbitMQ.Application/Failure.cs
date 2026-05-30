using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMQ.Application
{
    public static class Failure
    {
        public static async Task FailedMessages(AsyncEventingBasicConsumer consumer)
        {
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var headers = ea.BasicProperties.Headers;
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());

                Console.WriteLine("Poison messages");

                Console.WriteLine($"Body: {body}");
                Console.WriteLine($"Retry count: {headers?["x-retry-count"]}");
                Console.WriteLine($"Original queue: {headers?["x-original-queue"]}");
                Console.WriteLine($"Failure reason: {headers?["x-fail-reason"]}");
                Console.WriteLine($"Failed at: {headers?["x-failed-at"]}");
                Console.WriteLine();
            };

            await Task.CompletedTask;
        }
    }
}
