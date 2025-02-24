namespace EduInsights.Server.Enums;

public enum ErrorCode
{
    //default
    UnDefinedError,

    // Authentication & Authorization
    EmailNotVerified,
    InvalidCredentials,
    AccountLocked,
    AccessDenied,
    InvalidToken,
    TokenRequired,

    // Validation
    ValidationError,
    MissingRequiredField,
    InvalidEmailFormat,
    PasswordTooWeak,

    // User Management
    UserNotFound,
    UserAlreadyExists,
    InvalidUserRole,

    // Resource Management
    ResourceNotFound,
    ResourceAlreadyExists,
    ResourceLimitExceeded,

    // System & Server
    InternalServerError,
    ServiceUnavailable,
    DatabaseError,
    NetworkError,

    // Payment & Subscription
    PaymentFailed,
    SubscriptionExpired,
    InvalidPaymentMethod,

    // Third-Party Integration
    ThirdPartyApiError,
    ExternalServiceUnavailable,

    // Custom Business Logic
    InvalidOperation,
    QuotaExceeded,
    InvalidInvitationCode
}