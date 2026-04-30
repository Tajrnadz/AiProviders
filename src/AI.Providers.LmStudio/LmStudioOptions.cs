using AI.Abstractions.Configuration;

namespace AI.Providers.LmStudio;

/// <summary>Настройки LM Studio провайдера.</summary>
public sealed class LmStudioOptions : AiConnectionOptions
{
    public const string SectionName = "LmStudio";
    public const string DefaultBaseUrl = "http://localhost:1234/v1";

    public LmStudioOptions()
    {
        BaseUrl = DefaultBaseUrl;
    }

    /// <summary>Имя модели, загруженной в LM Studio.</summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>Максимальное количество токенов в ответе (0 — без ограничения).</summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Количество повторных попыток встроенного retry OpenAI SDK (по умолчанию 2).
    /// Установите 0, чтобы отключить retry на уровне SDK и управлять им через RetryMiddleware.
    /// </summary>
    public int SdkMaxRetries { get; set; } = 0;

    /// <summary>
    /// Таймаут сетевого запроса OpenAI SDK. Укажите в секундах.
    /// По умолчанию SDK использует 100 секунд. Для больших моделей рекомендуется 300–600.
    /// Значение 0 или отрицательное — оставить дефолт SDK.
    /// </summary>
    public int NetworkTimeoutSeconds { get; set; } = 0;
}
