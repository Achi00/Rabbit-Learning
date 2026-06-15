namespace RabbitMQ.Application.Enums
{
    public enum SagaStep
    {
        Started,
        StockReserved,
        PaymentCharged, 
        Completed, 
        Compensating,
        Cancelled
    }
}
