using AI.Abstractions.Configuration;

namespace AiPresentation;

/// <summary>
/// Конфигурация LM Studio провайдера. Читается из секции "LmStudio".
/// BaseUrl по умолчанию — http://localhost:1234/v1 (стандартный порт LM Studio).
/// ApiKey не требуется.
/// </summary>
public sealed class LmStudioProviderOptions
{
    public const string SectionName = "LmStudio";

    public string? ApiKey { get; set; }
    public string BaseUrl { get; set; } = "http://localhost:1234/v1";
    public string Model { get; set; } = "local-model";
    public string Prompt { get; set; } = "Say hello in one sentence.";
    public int MaxTokens { get; set; } = 4096;

    public AiProviderOptions ToAiProviderOptions() => new()
    {
        Provider = ProviderType.LmStudio,
        ApiKey   = ApiKey ?? string.Empty,
        BaseUrl  = BaseUrl
    };
}
