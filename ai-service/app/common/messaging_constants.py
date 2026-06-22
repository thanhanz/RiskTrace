class AnalysisMessagingConstants:
    EXCHANGE = "risktrace.events"
    
    #Queues
    DOCUMENT_UPLOADED_QUEUE = "risktrace.documents.uploaded"
    AI_RESPONSES_QUEUE = "risktrace.ai.responses"
    
    #Routing Keys
    DOCUMENT_UPLOADED_ROUTING_KEY = "document.uploaded_request"
    AI_REVIEW_COMPLETED_ROUTING_KEY = "ai.review_completed"
    TEMP_RESPONSE_EVENT_TYPE = "analysis.received"
