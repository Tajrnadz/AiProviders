using System.ClientModel;
using AI.Abstractions.Configuration;
using AI.Abstractions.Interfaces;
using AI.Abstractions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace AI.Providers.OpenAi;

public sealed class OpenAiProvider : IAiProvider
{
    private readonly AiProviderOptions _options;
    private readonly ILogger<OpenAiProvider>? _logger;

    public OpenAiProvider(
        IOptionsMonitor<AiProviderOptions> optionsMonitor,
        ILogger<OpenAiProvider>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(optionsMonitor);
        _options = optionsMonitor.CurrentValue;
        ValidateOptions(_options);
        _logger = logger;
    }

    private static void ValidateOptions(AiProviderOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
            throw new ArgumentException("OpenAI API key is required but not configured.", nameof(options));
    }

    private ChatClient BuildClient(string model)
    {
        var credential = new ApiKeyCredential(_options.ApiKey);

        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            var clientOptions = new OpenAIClientOptions
            {
                Endpoint = new Uri(_options.BaseUrl)
            };
            return new ChatClient(model, credential, clientOptions);
        }

        return new ChatClient(model, credential);
    }

    private static List<ChatMessage> MapMessages(IList<AiMessage> messages) =>
        messages.Select<AiMessage, ChatMessage>(m => m.Role switch
        {
            "system"    => new SystemChatMessage(m.Content),
            "assistant" => new AssistantChatMessage(m.Content),
            _           => new UserChatMessage(m.Content)
        }).ToList();

    /// <summary>
    /// Выполняет запрос к OpenAI Chat API и возвращает унифицированный AiResponse.
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

        _logger?.LogDebug("OpenAI request: Model={Model}, Messages={Count}", request.Model, messages.Count);

        var completion = await client.CompleteChatAsync(messages, chatOptions, cancellationToken);

        var text = completion.Value.Content[0].Text;
        var usage = completion.Value.Usage;

        _logger?.LogDebug("OpenAI response: InputTokens={Input}, OutputTokens={Output}",
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
