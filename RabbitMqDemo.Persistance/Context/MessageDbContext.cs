using Microsoft.EntityFrameworkCore;
using RabbitMq.Domain.Entity;

namespace RabbitMqDemo.Persistance.Context
{
    public class MessageDbContext : DbContext
    {
        public MessageDbContext(DbContextOptions options) : base(options) { }

        public DbSet<ProcessedMessages> Messages => Set<ProcessedMessages>();
        public DbSet<Orders> Orders => Set<Orders>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MessageDbContext).Assembly);
        }
    }
}
