namespace RabbitMq.Contracts
{
    public static class MessageTypes
    {
        public const string OrderCreated = "OrderCreated";
        public const string OrderCancelled = "OrderCancelled";

        public const string ReserveStock = "ReserveStock";
        public const string StockReserved = "StockReserved";
        public const string StockReservationFailed = "StockReservationFailed";

        public const string ReleaseStock = "ReleaseStock";
        public const string StockReleased = "StockReleased";

        public const string ChargePayment = "ChargePayment";
        public const string PaymentCharged = "PaymentCharged";
        public const string PaymentFailed = "PaymentFailed";
    }
}
