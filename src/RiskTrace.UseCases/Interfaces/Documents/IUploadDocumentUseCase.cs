namespace RiskTrace.UseCases.Interfaces.Documents;

public interface IUploadDocumentUseCase
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
