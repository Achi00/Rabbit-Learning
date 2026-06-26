namespace RabbitMq.Domain.Enums
{
    public enum OrderStatus
    {
        Submitted = 1,
        StockReserving = 2,
        PaymentCharging = 3,
        Compensating = 4,
        Completed = 5,
        Cancelled = 6
    }
}
