namespace AI.Abstractions.Models;

public sealed class AiRequest
{
    public string Model { get; init; } = string.Empty;
    public IList<AiMessage> Messages { get; init; } = [];
    public float? Temperature { get; init; }
    public int? MaxTokens { get; init; }
    public bool Stream { get; init; }
    public IDictionary<string, object> ProviderOptions { get; init; } = new Dictionary<string, object>();
}
