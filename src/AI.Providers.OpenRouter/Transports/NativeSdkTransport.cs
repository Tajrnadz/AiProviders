using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AI.Providers.OpenRouter.Configuration;
using AI.Abstractions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.Providers.OpenRouter.Transports;

/// <summary>
/// Реализует доступ к OpenRouter через нативный HttpClient,
/// что даёт полный доступ к OpenRouter-специфичным полям (route, fallbacks, transforms и др.).
/// </summary>
public sealed class NativeSdkTransport : IOpenRouterTransport
{
    private readonly OpenRouterOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<NativeSdkTransport>? _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public NativeSdkTransport(
        IOptionsMonitor<OpenRouterOptions> optionsMonitor,
        HttpClient httpClient,
        ILogger<NativeSdkTransport>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(optionsMonitor);
        ArgumentNullException.ThrowIfNull(httpClient);
        _options = optionsMonitor.CurrentValue;
        _httpClient = httpClient;
        _logger = logger;

        ValidateOptions(_options);
        ConfigureHttpClient();
    }

    private static void ValidateOptions(OpenRouterOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
            throw new ArgumentException("OpenRouter API key is required but not configured.", nameof(options));
    }

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + '/');
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        if (!string.IsNullOrWhiteSpace(_options.HttpReferer))
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("HTTP-Referer", _options.HttpReferer);

        if (!string.IsNullOrWhiteSpace(_options.AppTitle))
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Title", _options.AppTitle);

        if (_options.NetworkTimeoutSeconds > 0)
            _httpClient.Timeout = TimeSpan.FromSeconds(_options.NetworkTimeoutSeconds);
    }

    /// <summary>
    /// Выполняет запрос к OpenRouter через HttpClient и возвращает унифицированный AiResponse.
    /// </summary>
    public async Task<AiResponse> CompleteAsync(AiRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var body = BuildRequestBody(request);

        _logger?.LogDebug("OpenRouter (Native) request: Model={Model}, Messages={Count}",
            request.Model, request.Messages.Count);

        using var response = await _httpClient.PostAsJsonAsync(
            "chat/completions", body, JsonOptions, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenRouterResponse>(
            JsonOptions, cancellationToken);

        if (result is null)
            throw new InvalidOperationException("OpenRouter returned an empty response.");

        var text = result.Choices?[0].Message?.Content ?? string.Empty;

        _logger?.LogDebug("OpenRouter (Native) response: InputTokens={Input}, OutputTokens={Output}",
            result.Usage?.PromptTokens, result.Usage?.CompletionTokens);

        return new AiResponse
        {
            Content = text,
            Model = result.Model ?? request.Model,
            Usage = result.Usage is not null
                ? new AiUsage
                {
                    InputTokens = result.Usage.PromptTokens,
                    OutputTokens = result.Usage.CompletionTokens
                }
                : null,
            RawResponse = result
        };
    }

    public IAsyncEnumerable<AiChunk> StreamAsync(AiRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<AiModel>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private static OpenRouterRequest BuildRequestBody(AiRequest request)
    {
        // OpenRouter-специфичные поля можно передать через ProviderOptions
        request.ProviderOptions.TryGetValue("route", out var route);
        request.ProviderOptions.TryGetValue("transforms", out var transforms);

        return new OpenRouterRequest
        {
            Model = request.Model,
            Messages = request.Messages
                .Select(m => new OpenRouterMessage { Role = m.Role, Content = m.Content })
                .ToList(),
            Temperature = request.Temperature,
            MaxTokens = request.MaxTokens,
            Route = route as string,
            Transforms = transforms as List<string>
        };
    }

    // ---- Internal request/response DTOs ----

    private sealed class OpenRouterRequest
    {
        public string Model { get; init; } = string.Empty;
        public List<OpenRouterMessage> Messages { get; init; } = [];
        public float? Temperature { get; init; }
        public int? MaxTokens { get; init; }
        public string? Route { get; init; }
        public List<string>? Transforms { get; init; }
    }

    private sealed class OpenRouterMessage
    {
        public string Role { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;
    }

    private sealed class OpenRouterResponse
    {
        public string? Model { get; init; }
        public List<OpenRouterChoice>? Choices { get; init; }
        public OpenRouterUsage? Usage { get; init; }
    }

    private sealed class OpenRouterChoice
    {
        public OpenRouterMessage? Message { get; init; }
    }

    private sealed class OpenRouterUsage
    {
        public int PromptTokens { get; init; }
        public int CompletionTokens { get; init; }
    }
}
