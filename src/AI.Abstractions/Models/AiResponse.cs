namespace AI.Abstractions.Models;

public sealed class AiResponse
{
    public string Content { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public AiUsage? Usage { get; init; }
    public object? RawResponse { get; init; }
}
