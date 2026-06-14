namespace RabbitMQ.Application.Helpers
{
    public static class MessageRouting
    {
        // MessageType -> exchange, routing key
        private static readonly Dictionary<string, (string Exchange, string RoutingKey)> _routes = new()
        {
            ["OrderCreated"] = ("orders.exchange", "order.created"),
            ["OrderCancelled"] = ("orders.exchange", "order.cancelled"),
        };

        public static (string Exchange, string RoutingKey) Resolve(string messageType)
        => _routes.TryGetValue(messageType, out var route)
            ? route
            : throw new InvalidOperationException($"No route configured for '{messageType}'");
    }
}
