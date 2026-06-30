using Microsoft.EntityFrameworkCore;
using RabbitMq.Domain.Entity;
using RabbitMq.Infrastructure.Messaging.Saga;

namespace RabbitMqDemo.Persistance.Context
{
    public class MessageDbContext : DbContext
    {
        public MessageDbContext(DbContextOptions<MessageDbContext> options) : base(options) { }

        public DbSet<ProcessedMessage> Messages => Set<ProcessedMessage>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderSagaState> OrderSagaState => Set<OrderSagaState>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MessageDbContext).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderSagaState).Assembly);
        }
    }
}
