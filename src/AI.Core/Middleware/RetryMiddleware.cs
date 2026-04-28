using AI.Abstractions.Interfaces;
using AI.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace AI.Core.Middleware;

public sealed class RetryMiddleware : IAiMiddleware
{
    private readonly ILogger<RetryMiddleware> _logger;
    private readonly int _maxAttempts;
    private readonly TimeSpan _delay;

    public RetryMiddleware(ILogger<RetryMiddleware> logger, int maxAttempts = 3, TimeSpan delay = default)
    {
        _logger = logger;
        _maxAttempts = maxAttempts;
        _delay = delay == default ? TimeSpan.FromSeconds(1) : delay;
    }

    public async Task<AiResponse> InvokeAsync(
        AiRequest request,
        Func<AiRequest, CancellationToken, Task<AiResponse>> next,
        CancellationToken cancellationToken = default)
    {
        for (var attempt = 1; attempt <= _maxAttempts; attempt++)
        {
            try
            {
                return await next(request, cancellationToken);
            }
            catch (Exception ex) when (attempt < _maxAttempts && !cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "AI request failed on attempt {Attempt}/{Max}. Retrying...", attempt, _maxAttempts);
                await Task.Delay(_delay * attempt, cancellationToken);
            }
        }

        return await next(request, cancellationToken);
    }
}
