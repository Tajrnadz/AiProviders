namespace AiPresentation;

/// <summary>
/// Конфигурация OpenRouter провайдера. Читается из секции "Ai".
/// ApiKey хранится в user-secrets по ключу "Ai:ApiKey".
/// </summary>
public sealed class OpenRouterProviderOptions
{
    public string Model { get; set; } = "openai/gpt-4o-mini";
    public string Prompt { get; set; } = "Say hello in one sentence.";
}
