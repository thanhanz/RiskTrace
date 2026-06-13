using Microsoft.EntityFrameworkCore;
using RiskTrace.Domain.Entities;
using RiskTrace.UseCases.Ports.Repositories;

namespace RiskTrace.Infrastructure.Persistence.Repositories;

public sealed class MessageRepository(AppDbContext dbContext) : IMessageRepository
{
    public Task AddAsync(Message message, CancellationToken cancellationToken = default)
    {
        return dbContext.Messages.AddAsync(message, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyList<Message>> GetBySessionIdAsync(
        Guid sessionId,
        Guid? cursorId,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Messages
            .Where(message => message.SessionId == sessionId && message.IsActive);

        if (cursorId.HasValue)
        {
            query = query.Where(message => message.Id.CompareTo(cursorId.Value) < 0);
        }

        return await query
            .OrderByDescending(message => message.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
