# AiProviders — Architecture Documentation

## Overview

`AiProviders` — библиотека для унифицированного доступа к AI API различных провайдеров.  
Цель: предоставить единый интерфейс для работы с OpenAI, Google GenAI и OpenRouter,  
скрыв детали конкретных SDK за общим контрактом и обогатив вызовы cross-cutting логикой (валидация, retry, логирование).

---

## Solution Structure

```
AiProviders/
├── src/
│   ├── AI.Abstractions/                        # Контракты, модели, конфигурация
│   │   ├── Interfaces/
│   │   │   ├── IAiProvider.cs
│   │   │   └── IAiMiddleware.cs
│   │   ├── Models/
│   │   │   ├── AiMessage.cs
│   │   │   ├── AiRequest.cs
│   │   │   ├── AiResponse.cs
│   │   │   ├── AiChunk.cs
│   │   │   ├── AiModel.cs
│   │   │   └── AiUsage.cs
│   │   └── Configuration/
│   │       ├── AiProviderOptions.cs
│   │       ├── OpenRouterOptions.cs
│   │       ├── ProviderType.cs
│   │       └── OpenRouterTransportType.cs
│   │
│   ├── AI.Core/                                # Pipeline, middleware, общая бизнес-логика
│   │   ├── Pipeline/
│   │   │   └── AiPipelineProvider.cs
│   │   └── Middleware/
│   │       ├── ValidationMiddleware.cs
│   │       ├── LoggingMiddleware.cs
│   │       └── RetryMiddleware.cs
│   │
│   ├── AI.Providers.OpenAi/                    # Провайдер OpenAI
│   │   └── OpenAiProvider.cs                   ✅ CompleteAsync реализован
│   │
│   ├── AI.Providers.Google/                    # Провайдер Google GenAI
│   │   └── GoogleGenAiProvider.cs              ✅ CompleteAsync реализован
│   │
│   ├── AI.Providers.OpenRouter/                # Провайдер OpenRouter (два транспорта)
│   │   ├── OpenRouterProvider.cs
│   │   └── Transports/
│   │       ├── IOpenRouterTransport.cs
│   │       ├── OpenRouterHeadersPolicy.cs      # PipelinePolicy для HTTP-Referer / X-Title
│   │       ├── OpenAiSdkTransport.cs           ✅ CompleteAsync реализован
│   │       └── NativeSdkTransport.cs           ✅ CompleteAsync реализован
│   │
│   └── AI.Extensions.DI/                       # Регистрация в DI, фабрика
│       └── AiServiceCollectionExtensions.cs
│
├── ARCHITECTURE.md
└── BACKLOG.md
```

---

## Project Dependency Graph

```
AI.Abstractions
      ▲
      │ (reference)
      ├─────────────────────┬──────────────────────┬────────────────────┐
      │                     │                      │                    │
 AI.Core            AI.Providers.OpenAi   AI.Providers.Google  AI.Providers.OpenRouter
      ▲                     ▲                      ▲                    ▲
      │                     │                      │                    │
      └─────────────────────┴──────────────────────┴────────────────────┘
                                        │
                               AI.Extensions.DI
```

> **Важно:** провайдеры (`AI.Providers.*`) **не зависят** от `AI.Core`.  
> `AI.Core` знает только об `AI.Abstractions`.  
> `AI.Extensions.DI` — единственное место, где всё собирается вместе.

---

## Layers

### 1. AI.Abstractions

Чистые контракты без реализации. Не имеет зависимостей, кроме стандартной библиотеки .NET.

#### Interfaces

| Интерфейс | Описание |
|---|---|
| `IAiProvider` | Основной контракт провайдера: `CompleteAsync`, `StreamAsync`, `GetModelsAsync` |
| `IAiMiddleware` | Контракт middleware для pipeline: `InvokeAsync(request, next, ct)` |

#### Models

| Класс | Описание |
|---|---|
| `AiRequest` | Входной запрос: модель, сообщения, параметры, `ProviderOptions` |
| `AiResponse` | Ответ: контент, использованные токены, сырой ответ SDK |
| `AiMessage` | Сообщение с ролью (`system` / `user` / `assistant`) |
| `AiChunk` | Чанк стримингового ответа: `Delta`, `IsFinished` |
| `AiModel` | Описание доступной модели: `Id`, `DisplayName` |
| `AiUsage` | Статистика токенов: `InputTokens`, `OutputTokens`, `TotalTokens` |

#### Configuration

| Класс / Enum | Описание |
|---|---|
| `AiProviderOptions` | Секция конфигурации `"Ai"`: `Provider`, `ApiKey`, `BaseUrl`, `OpenRouterTransport` |
| `OpenRouterOptions` | Расширенные настройки OpenRouter: `ApiKey`, `BaseUrl`, `HttpReferer`, `AppTitle` |
| `ProviderType` | `OpenAi` \| `GoogleGenAi` \| `OpenRouter` |
| `OpenRouterTransportType` | `OpenAiSdk` \| `NativeSdk` |

---

### 2. AI.Core

Общая бизнес-логика, не зависящая от конкретного провайдера.

#### Pipeline

```
Request
   │
   ▼
[ValidationMiddleware]   — проверка модели, сообщений, параметров
   │
   ▼
[LoggingMiddleware]      — логирование запроса и ответа (модель, токены)
   │
   ▼
[RetryMiddleware]        — повтор при сетевых ошибках (exponential backoff)
   │
   ▼
IAiProvider (конкретная реализация)
```

`AiPipelineProvider` — декоратор над `IAiProvider`, выстраивающий цепочку middleware.  
Middleware применяются в порядке регистрации, аналогично ASP.NET Core pipeline.

#### Middleware

| Класс | Статус | Логика |
|---|---|---|
| `ValidationMiddleware` | ✅ | Проверяет `Model`, `Messages`, `MaxTokens`, `Temperature` |
| `LoggingMiddleware` | ✅ | `ILogger` — логирует запрос до и ответ после вызова провайдера |
| `RetryMiddleware` | ✅ | N попыток с задержкой (`attempt * delay`), пропускает `OperationCanceledException` |

---

### 3. AI.Providers.OpenAi

Реализация `IAiProvider` через официальный [OpenAI .NET SDK](https://github.com/openai/openai-dotnet) v2.10.0.

```
OpenAiProvider : IAiProvider
```

| Метод | Статус | Реализация |
|---|---|---|
| `CompleteAsync` | ✅ | `ChatClient.CompleteChatAsync` |
| `StreamAsync` | ⏳ | `ChatClient.CompleteChatStreamingAsync` |
| `GetModelsAsync` | ⏳ | `OpenAIModelClient.GetModelsAsync` |

**Конструктор:** принимает `IOptionsMonitor<AiProviderOptions>` и `ILogger<OpenAiProvider>`.  
**`BuildClient(model)`:** создаёт `ChatClient` с `ApiKeyCredential`; если задан `BaseUrl` — применяет `OpenAIClientOptions.Endpoint`.  
**`MapMessages()`:** `system` → `SystemChatMessage`, `assistant` → `AssistantChatMessage`, остальное → `UserChatMessage`.

---

### 4. AI.Providers.Google

Реализация `IAiProvider` через [Google GenAI .NET SDK](https://github.com/googleapis/dotnet-genai) v1.6.1 (Gemini Developer API).

```
GoogleGenAiProvider : IAiProvider
```

| Метод | Статус | Реализация |
|---|---|---|
| `CompleteAsync` | ✅ | `client.Models.GenerateContentAsync` |
| `StreamAsync` | ⏳ | `client.Models.GenerateContentStreamAsync` |
| `GetModelsAsync` | ⏳ | `client.Models.ListAsync` |

**Конструктор:** принимает `IOptionsMonitor<AiProviderOptions>` и `ILogger<GoogleGenAiProvider>`.  
**`BuildClient()`:** создаёт `Google.GenAI.Client` с `apiKey` и опциональным `HttpOptions.BaseUrl`.  
**`BuildConfig()`:** маппит `system`-сообщение в `SystemInstruction`, `Temperature`, `MaxTokens`.  
**`MapMessages()`:** `system` — пропускается, `assistant` → роль `"model"`, `user` → роль `"user"`.

---

### 5. AI.Providers.OpenRouter

OpenRouter поддерживает **два способа интеграции**, оба реализуют контракт `IOpenRouterTransport`.

```
OpenRouterProvider : IAiProvider
       │
       └── IOpenRouterTransport
               ├── OpenAiSdkTransport      — OpenAI SDK + BaseUrl = https://openrouter.ai/api/v1
               └── NativeSdkTransport      — HttpClient напрямую
```

#### OpenAiSdkTransport

| Метод | Статус | Реализация |
|---|---|---|
| `CompleteAsync` | ✅ | `ChatClient.CompleteChatAsync` (OpenAI SDK, endpoint → OpenRouter) |
| `StreamAsync` | ⏳ | `ChatClient.CompleteChatStreamingAsync` |
| `GetModelsAsync` | ⏳ | HTTP GET `/models` |

Заголовки `HTTP-Referer` и `X-Title` добавляются через `OpenRouterHeadersPolicy : PipelinePolicy`.

#### NativeSdkTransport

| Метод | Статус | Реализация |
|---|---|---|
| `CompleteAsync` | ✅ | `HttpClient.PostAsJsonAsync("chat/completions")` |
| `StreamAsync` | ⏳ | SSE через `HttpClient` с `HttpCompletionOption.ResponseHeadersRead` |
| `GetModelsAsync` | ⏳ | HTTP GET `/models` |

Поддерживает OpenRouter-эксклюзивные поля `route` и `transforms` через `AiRequest.ProviderOptions`.  
Все DTO (`OpenRouterRequest`, `OpenRouterResponse` и др.) — приватные, не утекают наружу.

#### Почему два транспорта?

| | OpenAiSdkTransport | NativeSdkTransport |
|---|---|---|
| **SDK** | `OpenAI` .NET SDK v2.10.0 | `HttpClient` |
| **Преимущество** | Минимум кода, типобезопасность SDK | Полный доступ к `route`, `fallbacks`, `transforms` |
| **Когда использовать** | Простые сценарии | Эксклюзивные функции OpenRouter |

---

### 6. AI.Extensions.DI

Точка входа для регистрации библиотеки в приложении.

```csharp
services.AddAiProvider(configuration);
```

Внутри:
1. Читает `AiProviderOptions` и `OpenRouterOptions` из секции `"Ai"` конфигурации
2. Регистрирует все middleware (`Validation`, `Logging`, `Retry`)
3. Регистрирует нужный `IAiProvider` по значению `ProviderType`
4. Для OpenRouter — регистрирует нужный `IOpenRouterTransport` по `OpenRouterTransportType`; для `NativeSdk` регистрирует `HttpClient` через `AddHttpClient<NativeSdkTransport>()`
5. Оборачивает `IAiProvider` в `AiPipelineProvider` через `Scrutor.Decorate`

---

## Configuration

`appsettings.json` — пример для каждого провайдера:

```json
// OpenAI
{
  "Ai": {
    "Provider": "OpenAi",
    "ApiKey": "sk-..."
  }
}

// Google GenAI (Gemini)
{
  "Ai": {
    "Provider": "GoogleGenAi",
    "ApiKey": "AIza..."
  }
}

// OpenRouter через OpenAI SDK
{
  "Ai": {
    "Provider": "OpenRouter",
    "ApiKey": "sk-or-...",
    "BaseUrl": "https://openrouter.ai/api/v1",
    "OpenRouterTransport": "OpenAiSdk",
    "HttpReferer": "https://myapp.com",
    "AppTitle": "MyApp"
  }
}

// OpenRouter через HttpClient
{
  "Ai": {
    "Provider": "OpenRouter",
    "ApiKey": "sk-or-...",
    "BaseUrl": "https://openrouter.ai/api/v1",
    "OpenRouterTransport": "NativeSdk",
    "HttpReferer": "https://myapp.com",
    "AppTitle": "MyApp"
  }
}
```

---

## Adding a New Provider

1. Создать проект `AI.Providers.MyProvider`
2. Добавить reference на `AI.Abstractions`
3. Реализовать `IAiProvider`
4. Добавить значение в enum `ProviderType`
5. Зарегистрировать в `AiServiceCollectionExtensions` в switch-блоке
6. Добавить reference в `AI.Extensions.DI`

---

## Adding a New Middleware

1. Создать класс в `AI.Core/Middleware/`, реализующий `IAiMiddleware`
2. Зарегистрировать в `AI.Extensions.DI` через `services.AddSingleton<MyMiddleware>()`
3. Добавить в список при построении `AiPipelineProvider`

---

## NuGet Dependencies

| Проект | Пакет | Версия | Назначение |
|---|---|---|---|
| `AI.Core` | `Microsoft.Extensions.Logging.Abstractions` | 10.0.7 | Логирование в middleware |
| `AI.Extensions.DI` | `Microsoft.Extensions.DependencyInjection.Abstractions` | 10.0.7 | DI контракты |
| `AI.Extensions.DI` | `Microsoft.Extensions.Options.ConfigurationExtensions` | 10.0.7 | Биндинг `appsettings.json` |
| `AI.Extensions.DI` | `Microsoft.Extensions.Http` | 10.0.7 | `IHttpClientFactory` для `NativeSdkTransport` |
| `AI.Extensions.DI` | `Scrutor` | 7.0.0 | `Decorate<IAiProvider>` для pipeline |
| `AI.Providers.OpenAi` | `OpenAI` | 2.10.0 | OpenAI .NET SDK |
| `AI.Providers.OpenAi` | `Microsoft.Extensions.Logging.Abstractions` | 10.0.7 | Логирование |
| `AI.Providers.OpenAi` | `Microsoft.Extensions.Options` | 10.0.7 | `IOptionsMonitor` |
| `AI.Providers.Google` | `Google.GenAI` | 1.6.1 | Gemini Developer API SDK |
| `AI.Providers.Google` | `Microsoft.Extensions.Logging.Abstractions` | 10.0.7 | Логирование |
| `AI.Providers.Google` | `Microsoft.Extensions.Options` | 10.0.7 | `IOptionsMonitor` |
| `AI.Providers.OpenRouter` | `OpenAI` | 2.10.0 | SDK для `OpenAiSdkTransport` |
| `AI.Providers.OpenRouter` | `Microsoft.Extensions.Logging.Abstractions` | 10.0.7 | Логирование |
| `AI.Providers.OpenRouter` | `Microsoft.Extensions.Options` | 10.0.7 | `IOptionsMonitor` |
