namespace RabbitMQ.Application.DTOs
{
    public class SubmitOrderRequest
    {
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string ConsumerEmail { get; set; }
    }
}
