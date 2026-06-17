using Microsoft.AspNetCore.Http;

namespace RiskTrace.Domain.Request.Documents;

public class InitiateDocumentUploadRequest
{
    public IFormFile File { get; set; }
}