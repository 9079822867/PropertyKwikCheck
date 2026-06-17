namespace PropertyKwikCheck.Core.Common;

/// <summary>
/// Domain/application error that maps to the spec's error envelope
/// <c>{ error, code, details }</c> with a specific HTTP status (spec §5).
/// </summary>
public class AppException : Exception
{
    public int StatusCode { get; }
    public string Code { get; }
    public IReadOnlyDictionary<string, string>? Details { get; }

    public AppException(int statusCode, string code, string message,
        IReadOnlyDictionary<string, string>? details = null) : base(message)
    {
        StatusCode = statusCode;
        Code = code;
        Details = details;
    }

    public static AppException NotFound(string message = "Lead not found") =>
        new(404, "NOT_FOUND", message);

    public static AppException Validation(string message, IReadOnlyDictionary<string, string>? details = null) =>
        new(400, "VALIDATION", message, details);

    public static AppException Unprocessable(string message, IReadOnlyDictionary<string, string>? details = null) =>
        new(422, "UNPROCESSABLE", message, details);

    public static AppException Unauthorized(string message = "Invalid credentials") =>
        new(401, "UNAUTHORIZED", message);

    public static AppException Forbidden(string message = "Forbidden") =>
        new(403, "RBAC_DENIED", message);

    public static AppException Conflict(string message, string code = "CONFLICT") =>
        new(409, code, message);

    public static AppException InvalidTransition(string from, string to) =>
        new(409, "INVALID_TRANSITION", $"Illegal stage transition: {from} → {to}");
}
