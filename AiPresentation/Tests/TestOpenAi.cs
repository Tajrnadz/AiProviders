using AI.Abstractions.Configuration;
using AI.Abstractions.Models;
using AI.Providers.OpenAi;
using Microsoft.Extensions.Options;

namespace AiPresentation.Tests;

public sealed class TestOpenAi(
    IOptions<OpenAiProviderOptions> options)
{
    private readonly OpenAiProviderOptions _options = options.Value;
    private readonly OpenAiProvider _provider = new(
        new StaticOptionsMonitor<AiProviderOptions>(
            options.Value.ToAiProviderOptions()));

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine();
        Console.WriteLine("=== Test: OpenAI ===");
        Console.WriteLine($"  Model  : {_options.Model}");
        Console.WriteLine($"  Prompt : {_options.Prompt}");
        Console.WriteLine();

        var request = new AiRequest
        {
            Model = _options.Model,
            Messages = [AiMessage.User(_options.Prompt)],
            MaxTokens = 256
        };

        var response = await _provider.CompleteAsync(request, cancellationToken);

        Console.WriteLine("--- Response ---");
        Console.WriteLine(response.Content);

        if (response.Usage is not null)
        {
            Console.WriteLine();
            Console.WriteLine($"  Tokens — input: {response.Usage.InputTokens}, " +
                              $"output: {response.Usage.OutputTokens}, " +
                              $"total: {response.Usage.TotalTokens}");
        }

        Console.WriteLine("=== Done ===");
        Console.WriteLine();
    }
}
