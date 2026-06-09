using Microsoft.EntityFrameworkCore;
using RabbitMq.Domain.Entity;

namespace RabbitMqDemo.Persistance.Context
{
    public class MessageDbContext : DbContext
    {
        public MessageDbContext(DbContextOptions<MessageDbContext> options) : base(options) { }

        public DbSet<ProcessedMessage> Messages => Set<ProcessedMessage>();
        public DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MessageDbContext).Assembly);
        }
    }
}
