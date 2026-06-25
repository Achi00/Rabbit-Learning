namespace RabbitMQ.Application.DTOs
{
    public class CreateOrderRequest
    {
        public decimal Amount { get; set; }
        public string CustomerEmail { get; set; }
    }
}
