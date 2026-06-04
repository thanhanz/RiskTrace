namespace RiskTrace.UseCases.Interfaces.Documents;

public interface IAnalyzeDocumentUseCase
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
