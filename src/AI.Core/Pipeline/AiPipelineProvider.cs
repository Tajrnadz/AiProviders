using AI.Abstractions.Interfaces;
using AI.Abstractions.Models;

namespace AI.Core.Pipeline;

/// <summary>
/// Wraps an IAiProvider with a middleware pipeline.
/// </summary>
public sealed class AiPipelineProvider : IAiProvider
{
    private readonly IAiProvider _inner;
    private readonly IReadOnlyList<IAiMiddleware> _middlewares;

    public AiPipelineProvider(IAiProvider inner, IEnumerable<IAiMiddleware> middlewares)
    {
        _inner = inner;
        _middlewares = [.. middlewares];
    }

    public async Task<AiResponse> CompleteAsync(AiRequest request, CancellationToken cancellationToken = default)
    {
        Func<AiRequest, CancellationToken, Task<AiResponse>> pipeline =
            (req, ct) => _inner.CompleteAsync(req, ct);

        foreach (var middleware in _middlewares.Reverse())
        {
            var next = pipeline;
            var current = middleware;
            pipeline = (req, ct) => current.InvokeAsync(req, next, ct);
        }

        return await pipeline(request, cancellationToken);
    }

    public IAsyncEnumerable<AiChunk> StreamAsync(AiRequest request, CancellationToken cancellationToken = default)
        => _inner.StreamAsync(request, cancellationToken);

    public Task<IReadOnlyList<AiModel>> GetModelsAsync(CancellationToken cancellationToken = default)
        => _inner.GetModelsAsync(cancellationToken);
}
