using AI.Abstractions.Models;

namespace AI.Providers.OpenRouter.Transports;

public interface IOpenRouterTransport
{
    Task<AiResponse> CompleteAsync(AiRequest request, CancellationToken cancellationToken = default);
    IAsyncEnumerable<AiChunk> StreamAsync(AiRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AiModel>> GetModelsAsync(CancellationToken cancellationToken = default);
}
