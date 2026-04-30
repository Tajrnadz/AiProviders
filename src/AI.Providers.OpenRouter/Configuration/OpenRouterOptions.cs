using AI.Abstractions.Configuration;

namespace AI.Providers.OpenRouter.Configuration;

public sealed class OpenRouterOptions : AiConnectionOptions
{
    public const string SectionName = "OpenRouter";
    public const string DefaultBaseUrl = "https://openrouter.ai/api/v1";

    public OpenRouterOptions()
    {
        BaseUrl = DefaultBaseUrl;
    }


    /// <summary>HTTP-Referer заголовок — идентифицирует приложение в OpenRouter dashboard.</summary>
    public string? HttpReferer { get; set; }

    /// <summary>X-Title заголовок — отображаемое имя приложения в OpenRouter dashboard.</summary>
    public string? AppTitle { get; set; }

    /// <summary>Тип транспорта: OpenAiSdk или NativeSdk.</summary>
    public OpenRouterTransportType Transport { get; set; } = OpenRouterTransportType.OpenAiSdk;

    /// <summary>Модель, используемая по умолчанию для всех запросов через OpenRouter.</summary>
    public string ModelName { get; set; } = "openai/gpt-4o-mini";

    /// <summary>
    /// Таймаут сетевого запроса в секундах.
    /// Для OpenAiSdk транспорта управляет <c>NetworkTimeout</c> OpenAI SDK (дефолт 100 с).
    /// Для NativeSdk транспорта управляет <c>HttpClient.Timeout</c>.
    /// Значение 0 или отрицательное — оставить дефолт.
    /// </summary>
    public int NetworkTimeoutSeconds { get; set; } = 0;
}
