using AI.Abstractions.Interfaces;
using AI.Abstractions.Models;

namespace AI.Core.Middleware;

public sealed class ValidationMiddleware : IAiMiddleware
{
    public Task<AiResponse> InvokeAsync(
        AiRequest request,
        Func<AiRequest, CancellationToken, Task<AiResponse>> next,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Model))
            throw new ArgumentException("Model must be specified.", nameof(request));

        if (request.Messages.Count == 0)
            throw new ArgumentException("Messages must not be empty.", nameof(request));

        if (request.MaxTokens is <= 0)
            throw new ArgumentException("MaxTokens must be greater than 0.", nameof(request));

        if (request.Temperature is < 0 or > 2)
            throw new ArgumentException("Temperature must be between 0 and 2.", nameof(request));

        return next(request, cancellationToken);
    }
}
