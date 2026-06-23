using System.Text.Json.Serialization;

namespace RiskTrace.Domain.Events;

public sealed record TemporaryAiResponseEvent
{
    [JsonPropertyName("event_type")]
    public string? EventType { get; init; }

    [JsonPropertyName("session_id")]
    public string? SessionId { get; init; }

    [JsonPropertyName("document_id")]
    public string? DocumentId { get; init; }

    [JsonPropertyName("occurred_at")]
    public string? OccurredAt { get; init; }

    [JsonPropertyName("payload")]
    public TemporaryAiResponsePayload? Payload { get; init; }
}

public sealed record TemporaryAiResponsePayload
{
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("user_id")]
    public string? UserId { get; init; }

    [JsonPropertyName("file_name")]
    public string? FileName { get; init; }

    [JsonPropertyName("storage_path")]
    public string? StoragePath { get; init; }
}
