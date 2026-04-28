using AI.Abstractions.Configuration;

namespace AiPresentation;

/// <summary>
/// Конфигурация OpenAI провайдера. Читается из секции "OpenAi".
/// ApiKey хранится в user-secrets по ключу "OpenAi:ApiKey".
/// </summary>
public sealed class OpenAiProviderOptions
{
    public const string SectionName = "OpenAi";

    public string ApiKey { get; set; } = string.Empty;
    public string? BaseUrl { get; set; }
    public string Model { get; set; } = "gpt-4o-mini";
    public string Prompt { get; set; } = "Say hello in one sentence.";

    public AiProviderOptions ToAiProviderOptions() => new()
    {
        Provider = ProviderType.OpenAi,
        ApiKey = ApiKey,
        BaseUrl = BaseUrl
    };
}
