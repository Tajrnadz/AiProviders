using AI.Extensions.DI;
using AiPresentation;
using AiPresentation.Tests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// appsettings.json добавляется автоматически через CreateApplicationBuilder.
// User-secrets и переменные окружения добавляются явно для секретов (ApiKey).
builder.Configuration
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

// AI provider (reads "Ai" section + user-secrets for ApiKey)
builder.Services.AddAiProvider(builder.Configuration);

// Test options
builder.Services.Configure<OpenRouterProviderOptions>(
    builder.Configuration.GetSection(AI.Providers.OpenRouter.Configuration.OpenRouterOptions.SectionName));
builder.Services.Configure<OpenAiProviderOptions>(
    builder.Configuration.GetSection(OpenAiProviderOptions.SectionName));
builder.Services.Configure<LmStudioProviderOptions>(
    builder.Configuration.GetSection(LmStudioProviderOptions.SectionName));

// Tests & menu
builder.Services.AddSingleton<TestOpenRouterOpenAi>();
builder.Services.AddSingleton<TestOpenAi>();
builder.Services.AddSingleton<TestLmStudio>();
builder.Services.AddSingleton<ConsoleMenu>();

var app = builder.Build();

var menu = app.Services.GetRequiredService<ConsoleMenu>();
await menu.RunAsync();
