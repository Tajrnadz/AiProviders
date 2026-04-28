using AI.Abstractions.Configuration;
using AI.Abstractions.Interfaces;
using AI.Abstractions.Models;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.Providers.Google;

public sealed class GoogleGenAiProvider : IAiProvider
{
    private readonly AiProviderOptions _options;
    private readonly ILogger<GoogleGenAiProvider>? _logger;

    public GoogleGenAiProvider(
        IOptionsMonitor<AiProviderOptions> optionsMonitor,
        ILogger<GoogleGenAiProvider>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(optionsMonitor);
        _options = optionsMonitor.CurrentValue;
        ValidateOptions(_options);
        _logger = logger;
    }

    private static void ValidateOptions(AiProviderOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
            throw new ArgumentException("Google GenAI API key is required but not configured.", nameof(options));
    }

    private Client BuildClient()
    {
        HttpOptions? httpOptions = null;

        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            httpOptions = new HttpOptions { BaseUrl = _options.BaseUrl };
        }

        return new Client(apiKey: _options.ApiKey, httpOptions: httpOptions);
    }

    private static GenerateContentConfig BuildConfig(AiRequest request)
    {
        var config = new GenerateContentConfig();

        var systemMessage = request.Messages.FirstOrDefault(m => m.Role == "system");
        if (systemMessage is not null)
        {
            config.SystemInstruction = new Content
            {
                Parts = [new Part { Text = systemMessage.Content }]
            };
        }

        if (request.Temperature.HasValue)
            config.Temperature = (double)request.Temperature.Value;

        if (request.MaxTokens.HasValue)
            config.MaxOutputTokens = request.MaxTokens.Value;

        return config;
    }

    private static IEnumerable<Content> MapMessages(IList<AiMessage> messages) =>
        messages
            .Where(m => m.Role != "system")
            .Select(m => new Content
            {
                Role = m.Role == "assistant" ? "model" : "user",
                Parts = [new Part { Text = m.Content }]
            });

    /// <summary>
    /// Выполняет запрос к Google Gemini и возвращает унифицированный AiResponse.
    /// </summary>
    public async Task<AiResponse> CompleteAsync(AiRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var client = BuildClient();
        var config = BuildConfig(request);
        var contents = MapMessages(request.Messages).ToList();

        _logger?.LogDebug("Google GenAI request: Model={Model}, Messages={Count}", request.Model, contents.Count);

        var response = await client.Models.GenerateContentAsync(
            model: request.Model,
            contents: contents,
            config: config);

        var text = response.Candidates?[0].Content?.Parts?[0].Text ?? string.Empty;
        var usage = response.UsageMetadata;

        _logger?.LogDebug("Google GenAI response: InputTokens={Input}, OutputTokens={Output}",
            usage?.PromptTokenCount, usage?.CandidatesTokenCount);

        return new AiResponse
        {
            Content = text,
            Model = request.Model,
            Usage = usage is not null
                ? new AiUsage
                {
                    InputTokens = usage.PromptTokenCount ?? 0,
                    OutputTokens = usage.CandidatesTokenCount ?? 0
                }
                : null,
            RawResponse = response
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
}
