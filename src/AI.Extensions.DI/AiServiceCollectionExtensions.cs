using AI.Abstractions.Configuration;
using AI.Abstractions.Interfaces;
using AI.Core.Middleware;
using AI.Core.Pipeline;
using AI.Providers.Google;
using AI.Providers.LmStudio;
using AI.Providers.OpenAi;
using AI.Providers.OpenRouter;
using AI.Providers.OpenRouter.Configuration;
using AI.Providers.OpenRouter.Transports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AI.Extensions.DI;

public static class AiServiceCollectionExtensions
{
    public static IServiceCollection AddAiProvider(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AiProviderOptions>(configuration.GetSection(AiProviderOptions.SectionName));
        services.Configure<OpenRouterOptions>(configuration.GetSection(OpenRouterOptions.SectionName));

        var options = configuration.GetSection(AiProviderOptions.SectionName).Get<AiProviderOptions>()
                      ?? new AiProviderOptions();

        var openRouterOptions = configuration.GetSection(OpenRouterOptions.SectionName).Get<OpenRouterOptions>()
                                ?? new OpenRouterOptions();

        // Register middleware
        services.AddSingleton<ValidationMiddleware>();
        services.AddSingleton<LoggingMiddleware>();
        services.AddSingleton<RetryMiddleware>();

        // Register concrete provider based on configuration
        switch (options.Provider)
        {
            case ProviderType.OpenAi:
                services.AddSingleton<IAiProvider, OpenAiProvider>();
                break;

            case ProviderType.GoogleGenAi:
                services.AddSingleton<IAiProvider, GoogleGenAiProvider>();
                break;

            case ProviderType.OpenRouter:
                RegisterOpenRouterTransport(services, openRouterOptions.Transport);
                services.AddSingleton<IAiProvider, OpenRouterProvider>();
                break;

            case ProviderType.LmStudio:
                services.AddSingleton<IAiProvider, LmStudioProvider>();
                break;

            default:
                throw new InvalidOperationException($"Unknown provider type: {options.Provider}");
        }

        // Wrap with pipeline
        services.Decorate<IAiProvider>((inner, sp) => new AiPipelineProvider(inner,
        [
            sp.GetRequiredService<ValidationMiddleware>(),
            sp.GetRequiredService<LoggingMiddleware>(),
            sp.GetRequiredService<RetryMiddleware>(),
        ]));

        return services;
    }

    private static void RegisterOpenRouterTransport(IServiceCollection services, OpenRouterTransportType transportType)
    {
        switch (transportType)
        {
            case OpenRouterTransportType.OpenAiSdk:
                services.AddSingleton<IOpenRouterTransport, OpenAiSdkTransport>();
                break;

            case OpenRouterTransportType.NativeSdk:
                services.AddHttpClient<NativeSdkTransport>();
                services.AddSingleton<IOpenRouterTransport, NativeSdkTransport>();
                break;

            default:
                throw new InvalidOperationException($"Unknown OpenRouter transport: {transportType}");
        }
    }
}
