using AI.Abstractions.Configuration;
using AI.Abstractions.Interfaces;
using AI.Abstractions.Models;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace AiPresentation.Tests;

public sealed class TestOpenRouterOpenAi(
    IAiProvider provider,
    IOptions<OpenRouterProviderOptions> options)
{
    private readonly OpenRouterProviderOptions _options = options.Value;

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold yellow]Test: OpenRouter via OpenAI SDK[/]").RuleStyle("yellow dim").LeftJustified());
        AnsiConsole.MarkupLine($"  [grey]Model  :[/] [cyan]{_options.Model}[/]");
        AnsiConsole.MarkupLine($"  [grey]Prompt :[/] [white]{_options.Prompt}[/]");
        AnsiConsole.WriteLine();

        var request = new AiRequest
        {
            Model = _options.Model,
            Messages =
            [
                AiMessage.User(_options.Prompt)
            ],
            MaxTokens = 256
        };

        var response = await provider.CompleteAsync(request, cancellationToken);

        AnsiConsole.Write(new Rule("[bold green]Response[/]").RuleStyle("green dim").LeftJustified());
        AnsiConsole.MarkupLine($"[green]{Markup.Escape(response.Content)}[/]");

        if (response.Usage is not null)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine(
                $"  [grey]Tokens —[/] " +
                $"[grey]input:[/] [yellow]{response.Usage.InputTokens}[/]  " +
                $"[grey]output:[/] [yellow]{response.Usage.OutputTokens}[/]  " +
                $"[grey]total:[/] [yellow]{response.Usage.TotalTokens}[/]");
        }

        AnsiConsole.Write(new Rule("[grey]Done[/]").RuleStyle("grey dim").LeftJustified());
        AnsiConsole.WriteLine();
    }
}
