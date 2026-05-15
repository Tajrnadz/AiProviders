using AI.Abstractions.Models;

namespace AI.Providers.OpenRouter.Transports;

public sealed partial class OpenAiSdkTransport
{
    internal static readonly IReadOnlyList<AiModel> PredefinedModels =
    [
        new() { Id = "anthropic/claude-opus-4.7",         DisplayName = "Claude Opus 4.7" },
        new() { Id = "x-ai/grok-4.20",                    DisplayName = "xAI: Grok 4.20" },
        new() { Id = "x-ai/grok-4.3",                     DisplayName = "xAI: Grok 4.3" },
        new() { Id = "openai/o3",                         DisplayName = "OpenAI: o3" },
        new() { Id = "~google/gemini-pro-latest",         DisplayName = "Google Gemini Pro Latest" },
        new() { Id = "qwen/qwen3.6-plus",                 DisplayName = "Qwen: Qwen3.6 Plus" },
        new() { Id = "openai/gpt-5.4",                    DisplayName = "OpenAI: GPT-5.4" },
    ];

    public Task<IReadOnlyList<AiModel>> GetModelsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(PredefinedModels);
}
