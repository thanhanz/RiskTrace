using RiskTrace.Domain.Entities;

namespace RiskTrace.UseCases.Ports.Repositories;

public interface IDocumentRepository
{
    Task AddAsync(Document document, CancellationToken cancellationToken = default);
}
