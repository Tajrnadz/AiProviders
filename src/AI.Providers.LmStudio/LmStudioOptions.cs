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
}
