using AI.Abstractions.Interfaces;
using AI.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace AI.Core.Middleware;

public sealed class LoggingMiddleware : IAiMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task<AiResponse> InvokeAsync(
        AiRequest request,
        Func<AiRequest, CancellationToken, Task<AiResponse>> next,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("AI request: Model={Model}, Messages={Count}", request.Model, request.Messages.Count);

        var response = await next(request, cancellationToken);

        _logger.LogDebug(
            "AI response: Model={Model}, InputTokens={Input}, OutputTokens={Output}",
            response.Model,
            response.Usage?.InputTokens,
            response.Usage?.OutputTokens);

        return response;
    }
}
