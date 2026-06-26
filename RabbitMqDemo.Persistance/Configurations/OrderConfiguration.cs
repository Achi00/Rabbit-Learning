using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RabbitMq.Domain.Entity;

namespace RabbitMqDemo.Persistance.Configurations
{
    public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.ConsumerEmail)
                .IsRequired()
                .HasMaxLength(250);
        }
    }
}
