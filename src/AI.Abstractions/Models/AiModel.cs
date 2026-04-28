namespace AI.Abstractions.Models;

public sealed class AiModel
{
    public string Id { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? Description { get; init; }
}
