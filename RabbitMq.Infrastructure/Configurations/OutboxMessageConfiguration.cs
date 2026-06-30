using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RabbitMq.Domain.Entity;

namespace RabbitMq.Infrastructure.Configurations
{
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.MessageType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Payload)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => x.SentAt);
        }
    }
}
