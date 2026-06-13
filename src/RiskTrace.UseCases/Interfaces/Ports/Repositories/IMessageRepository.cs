using RiskTrace.Domain.Entities;

namespace RiskTrace.UseCases.Ports.Repositories;

public interface IMessageRepository
{
    Task AddAsync(Message message, CancellationToken cancellationToken = default);
}
