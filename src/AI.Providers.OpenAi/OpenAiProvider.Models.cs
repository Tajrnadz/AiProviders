using AI.Abstractions.Models;

namespace AI.Providers.OpenAi;

public sealed partial class OpenAiProvider
{
    private static readonly IReadOnlyList<AiModel> PredefinedModels =
    [
        new() { Id = "gpt-4o",        DisplayName = "GPT-4o" },
        new() { Id = "gpt-4o-mini",   DisplayName = "GPT-4o Mini" },
        new() { Id = "gpt-4-turbo",   DisplayName = "GPT-4 Turbo" },
        new() { Id = "gpt-3.5-turbo", DisplayName = "GPT-3.5 Turbo" },
    ];

    public Task<IReadOnlyList<AiModel>> GetModelsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(PredefinedModels);
}
