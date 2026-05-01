using AI.Abstractions.Models;

namespace AI.Providers.Google;

public sealed partial class GoogleGenAiProvider
{
    private static readonly IReadOnlyList<AiModel> PredefinedModels =
    [
        new() { Id = "gemini-2.5-pro",  DisplayName = "Gemini 2.5 Pro" },
        new() { Id = "gemini-2.5-flash", DisplayName = "Gemini 2.5 Flash" },
        new() { Id = "gemini-2.0-flash", DisplayName = "Gemini 2.0 Flash" },
        new() { Id = "gemini-1.5-pro",   DisplayName = "Gemini 1.5 Pro" },
        new() { Id = "gemini-1.5-flash",  DisplayName = "Gemini 1.5 Flash" },
    ];

    public Task<IReadOnlyList<AiModel>> GetModelsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(PredefinedModels);
}
