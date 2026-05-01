using AI.Abstractions.Models;

namespace AI.Providers.OpenRouter.Transports;

public sealed partial class OpenAiSdkTransport
{
    internal static readonly IReadOnlyList<AiModel> PredefinedModels =
    [
        new() { Id = "anthropic/claude-opus-4.7",        DisplayName = "Claude Opus 4.7" },
        new() { Id = "anthropic/claude-sonnet-4-5",       DisplayName = "Claude Sonnet 4.5" },
        new() { Id = "anthropic/claude-3-5-haiku",        DisplayName = "Claude 3.5 Haiku" },
        new() { Id = "openai/gpt-4o",                     DisplayName = "GPT-4o (via OpenRouter)" },
        new() { Id = "openai/gpt-4o-mini",                DisplayName = "GPT-4o Mini (via OpenRouter)" },
        new() { Id = "google/gemini-2.5-pro",             DisplayName = "Gemini 2.5 Pro (via OpenRouter)" },
        new() { Id = "meta-llama/llama-3.3-70b-instruct", DisplayName = "Llama 3.3 70B Instruct" },
        new() { Id = "mistralai/mistral-large",           DisplayName = "Mistral Large" },
    ];

    public Task<IReadOnlyList<AiModel>> GetModelsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(PredefinedModels);
}
