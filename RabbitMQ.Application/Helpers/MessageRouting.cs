using RabbitMq.Contracts;

namespace RabbitMQ.Application.Helpers
{
    public static class MessageRouting
    {
        // MessageType -> exchange, routing key
        // adding one line here will work in RabbitMqPublisher
        private static readonly Dictionary<string, (string Exchange, string RoutingKey)> _routes = new()
        {
            [MessageTypes.OrderCreated] = ("orders.exchange", "order.created"),
            [MessageTypes.OrderCancelled] = ("orders.exchange", "order.cancelled"),
            [MessageTypes.ReserveStock] = ("orders.exchange", "saga.reserve-stock"),
            [MessageTypes.StockReserved] = ("orders.exchange", "saga.stock-reserved"),
            [MessageTypes.StockReservationFailed] = ("orders.exchange", "saga.stock-reservation-failed"),
            [MessageTypes.ChargePayment] = ("orders.exchange", "saga.charge-payment"),
            [MessageTypes.PaymentCharged] = ("orders.exchange", "saga.payment-charged"),
            [MessageTypes.PaymentFailed] = ("orders.exchange", "saga.payment-failed"),
            [MessageTypes.ReleaseStock] = ("orders.exchange", "saga.release-stock"),
            [MessageTypes.StockReleased] = ("orders.exchange", "saga.stock-released")
        };

        public static (string Exchange, string RoutingKey) Resolve(string messageType)
        => _routes.TryGetValue(messageType, out var route)
            ? route
            : throw new InvalidOperationException($"No route configured for '{messageType}'");
    }
}
