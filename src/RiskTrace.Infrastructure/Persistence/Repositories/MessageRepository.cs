using RiskTrace.Domain.Entities;
using RiskTrace.UseCases.Ports.Repositories;

namespace RiskTrace.Infrastructure.Persistence.Repositories;

public sealed class MessageRepository(AppDbContext dbContext) : IMessageRepository
{
    public Task AddAsync(Message message, CancellationToken cancellationToken = default)
    {
        return dbContext.Messages.AddAsync(message, cancellationToken).AsTask();
    }
}
