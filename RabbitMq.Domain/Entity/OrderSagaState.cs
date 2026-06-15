
using RabbitMQ.Application.Enums;

namespace RabbitMQ.Application.Models
{
    public class OrderSagaState
    {
        public Guid SagaId { get; set; }
        public SagaStep CurrentStep { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
