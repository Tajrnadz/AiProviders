using System.ClientModel;
using System.ClientModel.Primitives;
using AI.Providers.OpenRouter.Configuration;
using AI.Abstractions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace AI.Providers.OpenRouter.Transports;

/// <summary>
/// Реализует доступ к OpenRouter через официальный OpenAI .NET SDK,
/// переключая базовый URL на https://openrouter.ai/api/v1.
/// </summary>
public sealed class OpenAiSdkTransport : IOpenRouterTransport
{
    private readonly OpenRouterOptions _options;
    private readonly ILogger<OpenAiSdkTransport>? _logger;

    public OpenAiSdkTransport(
        IOptionsMonitor<OpenRouterOptions> optionsMonitor,
        ILogger<OpenAiSdkTransport>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(optionsMonitor);
        _options = optionsMonitor.CurrentValue;
        ValidateOptions(_options);
        _logger = logger;
    }

    private static void ValidateOptions(OpenRouterOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
            throw new ArgumentException("OpenRouter API key is required but not configured.", nameof(options));
    }

    private ChatClient BuildClient(string model)
    {
        var credential = new ApiKeyCredential(_options.ApiKey);

        var pipeline = new List<PipelinePolicy>();

        // Добавляем OpenRouter-специфичные заголовки если заданы
        if (!string.IsNullOrWhiteSpace(_options.HttpReferer) || !string.IsNullOrWhiteSpace(_options.AppTitle))
            pipeline.Add(new OpenRouterHeadersPolicy(_options.HttpReferer, _options.AppTitle));

        var clientOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri(_options.BaseUrl)
        };

        foreach (var policy in pipeline)
            clientOptions.AddPolicy(policy, PipelinePosition.PerCall);

        return new ChatClient(model, credential, clientOptions);
    }

    private static List<ChatMessage> MapMessages(IList<AiMessage> messages) =>
        messages.Select<AiMessage, ChatMessage>(m => m.Role switch
        {
            "system"    => new SystemChatMessage(m.Content),
            "assistant" => new AssistantChatMessage(m.Content),
            _           => new UserChatMessage(m.Content)
        }).ToList();

    /// <summary>
    /// Выполняет запрос к OpenRouter через OpenAI SDK и возвращает унифицированный AiResponse.
    /// </summary>
    public async Task<AiResponse> CompleteAsync(AiRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var client = BuildClient(request.Model);
        var messages = MapMessages(request.Messages);

        var chatOptions = new ChatCompletionOptions();
        if (request.Temperature.HasValue)
            chatOptions.Temperature = request.Temperature.Value;
        if (request.MaxTokens.HasValue)
            chatOptions.MaxOutputTokenCount = request.MaxTokens.Value;

        _logger?.LogDebug("OpenRouter (OpenAI SDK) request: Model={Model}, Messages={Count}",
            request.Model, messages.Count);

        var completion = await client.CompleteChatAsync(messages, chatOptions, cancellationToken);

        var text = completion.Value.Content[0].Text;
        var usage = completion.Value.Usage;

        _logger?.LogDebug("OpenRouter (OpenAI SDK) response: InputTokens={Input}, OutputTokens={Output}",
            usage?.InputTokenCount, usage?.OutputTokenCount);

        return new AiResponse
        {
            Content = text,
            Model = completion.Value.Model,
            Usage = usage is not null
                ? new AiUsage
                {
                    InputTokens = usage.InputTokenCount,
                    OutputTokens = usage.OutputTokenCount
                }
                : null,
            RawResponse = completion.Value
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
