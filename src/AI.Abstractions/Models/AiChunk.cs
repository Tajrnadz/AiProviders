namespace AI.Abstractions.Models;

public sealed class AiChunk
{
    public string Delta { get; init; } = string.Empty;
    public bool IsFinished { get; init; }
}
