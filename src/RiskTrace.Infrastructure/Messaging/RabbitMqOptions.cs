using RiskTrace.Domain.Messaging;

namespace RiskTrace.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 5672;

    public string VirtualHost { get; set; } = "/";

    public string Username { get; set; } = "guest";

    public string Password { get; set; } = "guest";

    public string ExchangeName { get; set; } = MessagingConstants.Exchanges.Main;

    //Queue to receive document indexing requests from the /document-indexing folder.
    public string DocumentQueue { get; set; } = MessagingConstants.Queues.DocumentUploaded;

    //Queue to receive AI responses from the /ai-service folder.
    public string AiResponseQueue { get; set; } = MessagingConstants.Queues.AiResponses;
}
