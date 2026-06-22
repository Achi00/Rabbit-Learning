using MassTransit;

namespace RabbitMq.Infrastructure.Messaging.Saga
{
    public class OrderStateMachine : MassTransitStateMachine<OrderSagaState>
    {
    }
}
