using AI.Abstractions.Models;

namespace AI.Abstractions.Interfaces;

public interface IAiProvider
{
    Task<AiResponse> CompleteAsync(AiRequest request, CancellationToken cancellationToken = default);
    IAsyncEnumerable<AiChunk> StreamAsync(AiRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AiModel>> GetModelsAsync(CancellationToken cancellationToken = default);
}
