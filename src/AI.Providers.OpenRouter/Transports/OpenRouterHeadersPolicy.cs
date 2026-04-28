using System.ClientModel.Primitives;

namespace AI.Providers.OpenRouter.Transports;

/// <summary>
/// Добавляет OpenRouter-специфичные заголовки HTTP-Referer и X-Title к каждому запросу.
/// </summary>
internal sealed class OpenRouterHeadersPolicy : PipelinePolicy
{
    private readonly string? _httpReferer;
    private readonly string? _appTitle;

    public OpenRouterHeadersPolicy(string? httpReferer, string? appTitle)
    {
        _httpReferer = httpReferer;
        _appTitle = appTitle;
    }

    public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        AddHeaders(message);
        ProcessNext(message, pipeline, currentIndex);
    }

    public override async ValueTask ProcessAsync(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        AddHeaders(message);
        await ProcessNextAsync(message, pipeline, currentIndex);
    }

    private void AddHeaders(PipelineMessage message)
    {
        if (!string.IsNullOrWhiteSpace(_httpReferer))
            message.Request.Headers.Set("HTTP-Referer", _httpReferer);

        if (!string.IsNullOrWhiteSpace(_appTitle))
            message.Request.Headers.Set("X-Title", _appTitle);
    }
}
