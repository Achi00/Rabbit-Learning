using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RabbitMq.Domain.Entity;

namespace RabbitMqDemo.Persistance.Configurations
{
    public sealed class ProcessedMessageConfiguration : IEntityTypeConfiguration<ProcessedMessage>
    {
        public void Configure(EntityTypeBuilder<ProcessedMessage> builder)
        {
            builder.ToTable("ProcessedMessages");

            builder.HasKey(x => x.MessageId);

            builder.Property(x => x.MessageId)
                .IsRequired();

            builder.Property(x => x.MessageType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.ProcessedAt)
                .IsRequired();
        }
    }
}
