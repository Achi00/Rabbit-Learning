using RabbitMQ.Application.Enums;

namespace RabbitMQ.Application.Models
{
    public class SagaState
    {
        public Guid SagaId { get; set; }
        public SagaStep CurrentStep { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
