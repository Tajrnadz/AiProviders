using AI.Abstractions.Models;

namespace AI.Providers.OpenRouter.Transports;

public sealed partial class NativeSdkTransport
{
    public Task<IReadOnlyList<AiModel>> GetModelsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(OpenAiSdkTransport.PredefinedModels);
}
