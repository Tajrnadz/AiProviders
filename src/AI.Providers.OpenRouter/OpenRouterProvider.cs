using AI.Abstractions.Interfaces;
using AI.Abstractions.Models;
using AI.Providers.OpenRouter.Transports;

namespace AI.Providers.OpenRouter;

public sealed class OpenRouterProvider : IAiProvider
{
    private readonly IOpenRouterTransport _transport;

    public OpenRouterProvider(IOpenRouterTransport transport)
    {
        _transport = transport;
    }

    public Task<AiResponse> CompleteAsync(AiRequest request, CancellationToken cancellationToken = default)
        => _transport.CompleteAsync(request, cancellationToken);

    public IAsyncEnumerable<AiChunk> StreamAsync(AiRequest request, CancellationToken cancellationToken = default)
        => _transport.StreamAsync(request, cancellationToken);

    public Task<IReadOnlyList<AiModel>> GetModelsAsync(CancellationToken cancellationToken = default)
        => _transport.GetModelsAsync(cancellationToken);
}
