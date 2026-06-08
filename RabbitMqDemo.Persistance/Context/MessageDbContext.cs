using Microsoft.EntityFrameworkCore;

namespace RabbitMqDemo.Persistance.Context
{
    public class MessageDbContext : DbContext
    {
        public MessageDbContext(DbContextOptions options) : base(options) { }

        public DbSet<> MyProperty { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MessageDbContext).Assembly);
        }
    }
}
