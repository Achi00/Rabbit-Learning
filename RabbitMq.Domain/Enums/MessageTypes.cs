namespace RabbitMQ.Application.Enums
{
    public enum MessageTypes
    {
        // order
        OrderCreated,
        OrderCancelled,
        // stock status
        ReserveStock,
        StockReserved,
        StockReservationFailed,
        ReleaseStock,
        // payment status
        ChargePayment,
        PaymentCharged,
        PaymentFailed
    }
}
