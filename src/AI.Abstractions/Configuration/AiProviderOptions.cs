namespace AI.Abstractions.Configuration;

public sealed class AiProviderOptions : AiConnectionOptions
{
    public const string SectionName = "Ai";

    public ProviderType Provider { get; set; } = ProviderType.OpenAi;
}
