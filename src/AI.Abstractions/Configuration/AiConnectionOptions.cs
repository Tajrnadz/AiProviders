namespace AI.Abstractions.Configuration;

/// <summary>Общие параметры подключения к AI провайдеру: ключ и базовый URL.</summary>
public abstract class AiConnectionOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string? BaseUrl { get; set; }
}
