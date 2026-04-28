namespace AI.Abstractions.Models;

public sealed class AiMessage
{
    public string Role { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;

    public static AiMessage System(string content) => new() { Role = "system", Content = content };
    public static AiMessage User(string content) => new() { Role = "user", Content = content };
    public static AiMessage Assistant(string content) => new() { Role = "assistant", Content = content };
}
