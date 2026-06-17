using System.Text.Json;
using System.Text.Json.Serialization;
using PropertyKwikCheck.Core.Common;

namespace PropertyKwikCheck.Api.Middleware;

/// <summary>
/// Translates exceptions into the spec's error envelope <c>{ error, code, details }</c>
/// (spec §5). Never leaks stack traces in production.
/// </summary>
public sealed class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            await WriteAsync(context, ex.StatusCode, ex.Message, ex.Code, ex.Details);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteAsync(context, 500, "An unexpected error occurred", "INTERNAL", null);
        }
    }

    private static async Task WriteAsync(HttpContext context, int status, string error, string code,
        IReadOnlyDictionary<string, string>? details)
    {
        if (context.Response.HasStarted) return;
        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        var body = new ErrorBody { Error = error, Code = code, Details = details };
        await context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }

    private sealed class ErrorBody
    {
        [JsonPropertyName("error")] public string Error { get; set; } = "";
        [JsonPropertyName("code")] public string Code { get; set; } = "";

        [JsonPropertyName("details")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IReadOnlyDictionary<string, string>? Details { get; set; }
    }
}
