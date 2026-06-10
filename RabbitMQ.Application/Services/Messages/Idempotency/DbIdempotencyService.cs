using Microsoft.EntityFrameworkCore;
using RabbitMq.Domain.Entity;
using RabbitMqDemo.Persistance.Context;

namespace RabbitMQ.Application.Services.Messages.Idempotency
{
    public class DbIdempotencyService
    {
        private readonly MessageDbContext _context;

        public DbIdempotencyService(MessageDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IdDuplicateAsync(Guid messageId)
        {
            return await _context.Messages.AnyAsync(m => m.MessageId == messageId);
        }

        public void MarkAsProcessed(Guid messageId, string messageType)
        {
            _context.Messages.Add(new ProcessedMessage 
            { 
                MessageId = messageId,
                MessageType = messageType,
                ProcessedAt = DateTimeOffset.UtcNow
            });
            // no save changes here caller controlls transaction
        }
    }
}
