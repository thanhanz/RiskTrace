class AiServiceError(Exception):
    """Base exception for expected AI service failures."""


class MessageHandlingError(AiServiceError):
    """Raised when an incoming event cannot be handled."""
