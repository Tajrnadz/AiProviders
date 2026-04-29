using AI.Abstractions.Configuration;
using AI.Abstractions.Models;
using AI.Providers.LmStudio;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace AiPresentation.Tests;

public sealed class TestLmStudio(
    IOptions<LmStudioProviderOptions> options)
{
    private readonly LmStudioProviderOptions _options = options.Value;
    private readonly LmStudioProvider _provider = new(
        new StaticOptionsMonitor<AiProviderOptions>(
            options.Value.ToAiProviderOptions()));

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold blue]Test: LM Studio (local)[/]").RuleStyle("blue dim").LeftJustified());
        AnsiConsole.MarkupLine($"  [grey]BaseUrl:[/] [cyan]{_options.BaseUrl}[/]");
        AnsiConsole.MarkupLine($"  [grey]Model  :[/] [cyan]{_options.Model}[/]");
        AnsiConsole.MarkupLine($"  [grey]Prompt :[/] [white]{Markup.Escape(_options.Prompt)}[/]");
        AnsiConsole.WriteLine();

        var request = new AiRequest
        {
            Model     = _options.Model,
            Messages  = [AiMessage.User(_options.Prompt)],
            MaxTokens = _options.MaxTokens
        };

        try
        {
            var response = await _provider.CompleteAsync(request, cancellationToken);

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
        }
        catch (Exception ex)
        {
            AnsiConsole.Write(new Rule("[bold red]Error[/]").RuleStyle("red dim").LeftJustified());
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]");
            AnsiConsole.MarkupLine("[grey]Make sure LM Studio is running and a model is loaded.[/]");
        }

        AnsiConsole.Write(new Rule("[grey]Done[/]").RuleStyle("grey dim").LeftJustified());
        AnsiConsole.WriteLine();
    }
}
