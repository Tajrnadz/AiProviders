using AI.Abstractions.Models;

namespace AI.Providers.LmStudio;

public sealed partial class LmStudioProvider
{
    private static readonly IReadOnlyList<AiModel> PredefinedModels =
    [
        new() { Id = "gemma-4-e4b-it",         DisplayName = "Gemma 4 E4B IT" },
        new() { Id = "llama-3.2-3b-instruct",   DisplayName = "Llama 3.2 3B Instruct" },
        new() { Id = "llama-3.1-8b-instruct",   DisplayName = "Llama 3.1 8B Instruct" },
        new() { Id = "mistral-7b-instruct",     DisplayName = "Mistral 7B Instruct" },
        new() { Id = "phi-4",                   DisplayName = "Phi-4" },
        new() { Id = "qwen2.5-7b-instruct",     DisplayName = "Qwen 2.5 7B Instruct" },
    ];

    public Task<IReadOnlyList<AiModel>> GetModelsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(PredefinedModels);
}
