using RiskTrace.Domain.Entities;
using RiskTrace.UseCases.Ports.Repositories;

namespace RiskTrace.Infrastructure.Persistence.Repositories;

public sealed class DocumentRepository(AppDbContext dbContext) : IDocumentRepository
{
    public Task AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        return dbContext.Documents.AddAsync(document, cancellationToken).AsTask();
    }
}
