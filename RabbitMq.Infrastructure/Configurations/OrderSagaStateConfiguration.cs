using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RabbitMq.Infrastructure.Messaging.Saga;

namespace RabbitMq.Infrastructure.Configurations
{
    public class OrderSagaStateConfiguration : IEntityTypeConfiguration<OrderSagaState>
    {
        public void Configure(EntityTypeBuilder<OrderSagaState> builder)
        {
            builder.ToTable("OrderSagaState");
            builder.HasKey(x => x.CorrelationId);

            builder.Property(x => x.CurrentState)
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(x => x.OrderId).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.UpdatedAt).IsRequired();
            builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.CustomerEmail).HasMaxLength(256);
        }
    }
}
