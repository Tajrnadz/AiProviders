using System.ClientModel;
using System.ClientModel.Primitives;
using AI.Abstractions.Interfaces;
using AI.Abstractions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace AI.Providers.LmStudio;

/// <summary>
/// Провайдер LM Studio — локальный OpenAI-совместимый сервер.
/// По умолчанию доступен на http://localhost:1234/v1.
/// API-ключ не требуется; если не задан — используется заглушка "lm-studio".
/// </summary>
public sealed partial class LmStudioProvider : IAiProvider
{
    private const string DummyApiKey = "lm-studio";

    private readonly LmStudioOptions _options;
    private readonly ILogger<LmStudioProvider>? _logger;

    public LmStudioProvider(
        IOptions<LmStudioOptions> options,
        ILogger<LmStudioProvider>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
        _logger  = logger;
    }

    private ChatClient BuildClient(string model)
    {
        var apiKey  = string.IsNullOrWhiteSpace(_options.ApiKey) ? DummyApiKey : _options.ApiKey;
        var baseUrl = string.IsNullOrWhiteSpace(_options.BaseUrl) ? LmStudioOptions.DefaultBaseUrl : _options.BaseUrl;

        var clientOptions = new OpenAIClientOptions
        {
            Endpoint    = new Uri(baseUrl),
            RetryPolicy = new ClientRetryPolicy(_options.SdkMaxRetries)
        };

        if (_options.NetworkTimeoutSeconds > 0)
            clientOptions.NetworkTimeout = TimeSpan.FromSeconds(_options.NetworkTimeoutSeconds);

        return new ChatClient(model, new ApiKeyCredential(apiKey), clientOptions);
    }

    private static List<ChatMessage> MapMessages(IList<AiMessage> messages) =>
        messages.Select<AiMessage, ChatMessage>(m => m.Role switch
        {
            "system"    => new SystemChatMessage(m.Content),
            "assistant" => new AssistantChatMessage(m.Content),
            _           => new UserChatMessage(m.Content)
        }).ToList();

    public async Task<AiResponse> CompleteAsync(AiRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var client   = BuildClient(request.Model);
        var messages = MapMessages(request.Messages);

        var chatOptions = new ChatCompletionOptions();
        if (request.Temperature.HasValue)
            chatOptions.Temperature = request.Temperature.Value;

        var maxTokens = request.MaxTokens ?? _options.MaxTokens;
        if (maxTokens.HasValue)
            chatOptions.MaxOutputTokenCount = maxTokens.Value;

        _logger?.LogDebug("LmStudio request: Model={Model}, Messages={Count}", request.Model, messages.Count);

        var completion = await client.CompleteChatAsync(messages, chatOptions, cancellationToken);

        var text  = completion.Value.Content[0].Text;
        var usage = completion.Value.Usage;

        _logger?.LogDebug("LmStudio response: InputTokens={Input}, OutputTokens={Output}",
            usage?.InputTokenCount, usage?.OutputTokenCount);

        return new AiResponse
        {
            Content     = text,
            Model       = completion.Value.Model,
            Usage       = usage is not null
                ? new AiUsage
                {
                    InputTokens  = usage.InputTokenCount,
                    OutputTokens = usage.OutputTokenCount
                }
                : null,
            RawResponse = completion.Value
        };
    }

    public IAsyncEnumerable<AiChunk> StreamAsync(AiRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
