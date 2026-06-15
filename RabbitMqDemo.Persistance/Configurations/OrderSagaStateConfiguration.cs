using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RabbitMQ.Application.Models;

namespace RabbitMqDemo.Persistance.Configurations
{
    public class OrderSagaStateConfiguration : IEntityTypeConfiguration<OrderSagaState>
    {
        public void Configure(EntityTypeBuilder<OrderSagaState> builder)
        {
            builder.ToTable("OrderSagaState");

            builder.HasKey(x => x.SagaId);

            builder.Property(x => x.CurrentStep)
                // will store enum as string
                .HasConversion<string>()
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();
            
            builder.Property(x => x.UpdatedAt)
                .IsRequired();
        }
    }
}
