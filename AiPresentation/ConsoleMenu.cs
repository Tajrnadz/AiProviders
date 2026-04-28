using AiPresentation.Tests;
using Spectre.Console;

namespace AiPresentation;

public sealed class ConsoleMenu(
    TestOpenRouterOpenAi testOpenRouterOpenAi,
    TestOpenAi testOpenAi)
{
    private const string ItemOpenRouter = "OpenRouter via OpenAI SDK";
    private const string ItemOpenAi = "OpenAI";
    private const string ItemExit = "Exit";

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        PrintHeader();

        while (!cancellationToken.IsCancellationRequested)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[grey]Use[/] [yellow]↑↓[/] [grey]to navigate,[/] [yellow]Enter[/] [grey]to select:[/]")
                    .HighlightStyle(new Style(foreground: Color.Cyan1, decoration: Decoration.Bold))
                    .AddChoices(ItemOpenRouter, ItemOpenAi, ItemExit));

            if (choice == ItemExit)
            {
                AnsiConsole.MarkupLine("[grey]Exiting...[/]");
                return;
            }

            AnsiConsole.WriteLine();

            if (choice == ItemOpenRouter)
                await testOpenRouterOpenAi.RunAsync(cancellationToken);
            else if (choice == ItemOpenAi)
                await testOpenAi.RunAsync(cancellationToken);

            AnsiConsole.WriteLine();
        }
    }

    private static void PrintHeader()
    {
        AnsiConsole.Write(
            new FigletText("AiProviders")
                .Centered()
                .Color(Color.Cyan1));

        AnsiConsole.Write(
            new Rule("[bold cyan]Test Menu[/]")
                .RuleStyle(Style.Parse("cyan dim"))
                .Centered());

        AnsiConsole.WriteLine();
    }
}
