namespace RiskTrace.UseCases.Interfaces.Documents;

public interface IDeleteDocumentUseCase
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
