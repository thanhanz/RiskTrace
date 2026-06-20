namespace RiskTrace.Domain.Messaging;

public static class MessagingConstants
{
    public static class Exchanges
    {
        public const string Main = "risktrace.events";
    }

    public static class Queues
    {
        public const string DocumentUploaded = "risktrace.documents.uploaded";
        public const string AiResponses = "risktrace.ai.responses";
    }

    public static class RoutingKeys
    {
        public const string DocumentUploadedRequest = "document.uploaded_request";
        public const string AiReviewCompleted = "ai.review_completed";
    }
}
