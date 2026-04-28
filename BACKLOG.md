# AiProviders — Backlog

Список невыполненных задач, сгруппированных по приоритету и области.  
Статусы: 🔴 не начато · 🟡 частично · ✅ выполнено

---

## 🏁 Phase 1 — Core Completion (высокий приоритет)

### 1.1 StreamAsync — стриминговые ответы

| # | Задача | Провайдер / файл | Статус |
|---|---|---|---|
| 1 | Реализовать `StreamAsync` | `OpenAiProvider.cs` | 🔴 |
| 2 | Реализовать `StreamAsync` | `GoogleGenAiProvider.cs` | 🔴 |
| 3 | Реализовать `StreamAsync` | `OpenAiSdkTransport.cs` | 🔴 |
| 4 | Реализовать `StreamAsync` через SSE + `HttpClient` | `NativeSdkTransport.cs` | 🔴 |
| 5 | Добавить поддержку стриминга в pipeline | `AiPipelineProvider.cs` | 🔴 |

**Детали реализации:**
- `OpenAiProvider` → `client.CompleteChatStreamingAsync` → `yield return new AiChunk`
- `GoogleGenAiProvider` → `client.Models.GenerateContentStreamAsync` → `await foreach`
- `OpenAiSdkTransport` → аналогично `OpenAiProvider`
- `NativeSdkTransport` → `HttpCompletionOption.ResponseHeadersRead` + разбор SSE (`data: {...}`)
- `AiPipelineProvider.StreamAsync` — middleware не оборачивает стриминг, делегирует напрямую в `_inner`

---

### 1.2 GetModelsAsync — список доступных моделей

| # | Задача | Провайдер / файл | Статус |
|---|---|---|---|
| 6 | Реализовать `GetModelsAsync` | `OpenAiProvider.cs` | 🔴 |
| 7 | Реализовать `GetModelsAsync` | `GoogleGenAiProvider.cs` | 🔴 |
| 8 | Реализовать `GetModelsAsync` | `OpenAiSdkTransport.cs` | 🔴 |
| 9 | Реализовать `GetModelsAsync` через HTTP GET `/models` | `NativeSdkTransport.cs` | 🔴 |

**Детали реализации:**
- `OpenAiProvider` → `OpenAIModelClient.GetModelsAsync()` → маппинг в `AiModel`
- `GoogleGenAiProvider` → `client.Models.ListAsync()` → `await foreach` по `Pager`
- `OpenAiSdkTransport` → аналогично `OpenAiProvider`
- `NativeSdkTransport` → `GET /models` → десериализация в `List<AiModel>`

---

## 🧪 Phase 2 — Testing (высокий приоритет)

### 2.1 Unit-тесты

| # | Задача | Файл | Статус |
|---|---|---|---|
| 10 | Создать проект `AI.Tests.Unit` | `AI.Tests.Unit.csproj` | 🔴 |
| 11 | Тесты `ValidationMiddleware` — граничные случаи | `ValidationMiddlewareTests.cs` | 🔴 |
| 12 | Тесты `RetryMiddleware` — кол-во попыток, отмена | `RetryMiddlewareTests.cs` | 🔴 |
| 13 | Тесты `AiPipelineProvider` — порядок middleware | `AiPipelineProviderTests.cs` | 🔴 |
| 14 | Тесты маппинга сообщений `OpenAiProvider` | `OpenAiProviderTests.cs` | 🔴 |
| 15 | Тесты маппинга сообщений `GoogleGenAiProvider` | `GoogleGenAiProviderTests.cs` | 🔴 |
| 16 | Тесты `NativeSdkTransport` — сериализация DTO | `NativeSdkTransportTests.cs` | 🔴 |

**Стек:** `xUnit` + `Moq` + `Microsoft.AspNetCore.Mvc.Testing` (при необходимости)

### 2.2 Integration-тесты

| # | Задача | Статус |
|---|---|---|
| 17 | Создать проект `AI.Tests.Integration` | 🔴 |
| 18 | Тест реального вызова `OpenAiProvider.CompleteAsync` (с реальным ключом через `dotnet user-secrets`) | 🔴 |
| 19 | Тест реального вызова `GoogleGenAiProvider.CompleteAsync` | 🔴 |
| 20 | Тест реального вызова `OpenRouterProvider` (оба транспорта) | 🔴 |

---

## 🔧 Phase 3 — Robustness (средний приоритет)

### 3.1 Обработка ошибок

| # | Задача | Статус |
|---|---|---|
| 21 | Ввести типизированные исключения: `AiProviderException`, `AiRateLimitException`, `AiAuthException` | 🔴 |
| 22 | Обернуть SDK-исключения в типизированные в каждом провайдере | 🔴 |
| 23 | В `RetryMiddleware` — retry только на `AiRateLimitException` и сетевые ошибки, не на auth | 🔴 |
| 24 | Добавить `CancellationToken` propagation проверку в `NativeSdkTransport` | 🔴 |

### 3.2 Валидация конфигурации

| # | Задача | Статус |
|---|---|---|
| 25 | Добавить `IValidateOptions<AiProviderOptions>` с подробными сообщениями об ошибках | 🔴 |
| 26 | Добавить `IValidateOptions<OpenRouterOptions>` | 🔴 |
| 27 | Вызывать валидацию при старте приложения через `services.AddOptions<...>().ValidateOnStart()` | 🔴 |

### 3.3 Resilience

| # | Задача | Статус |
|---|---|---|
| 28 | Заменить ручной retry в `RetryMiddleware` на `Microsoft.Extensions.Resilience` (Polly v8) | 🔴 |
| 29 | Добавить `CircuitBreaker` политику | 🔴 |
| 30 | Добавить `Timeout` политику на уровне middleware | 🔴 |

---

## 🚀 Phase 4 — Features (низкий приоритет)

### 4.1 Кэширование ответов

| # | Задача | Статус |
|---|---|---|
| 31 | Создать `CacheMiddleware : IAiMiddleware` | 🔴 |
| 32 | Реализовать `IAiCacheKeyStrategy` — стратегия построения ключа кэша (модель + хэш сообщений) | 🔴 |
| 33 | Подключить `IMemoryCache` / `IDistributedCache` по конфигурации | 🔴 |

### 4.2 Подсчёт токенов

| # | Задача | Статус |
|---|---|---|
| 34 | Создать `ITokenCounter` интерфейс в `AI.Abstractions` | 🔴 |
| 35 | Реализовать `TiktokenCounter` для OpenAI-совместимых моделей | 🔴 |
| 36 | Реализовать `GoogleTokenCounter` через `client.Models.CountTokensAsync` | 🔴 |

### 4.3 Телеметрия

| # | Задача | Статус |
|---|---|---|
| 37 | Добавить `Activity` (OpenTelemetry) в `LoggingMiddleware` | 🔴 |
| 38 | Добавить метрики (`IMetrics`): кол-во запросов, латентность, токены | 🔴 |
| 39 | Добавить `AI.Extensions.Telemetry` проект с регистрацией OpenTelemetry | 🔴 |

### 4.4 Множественные провайдеры одновременно

| # | Задача | Статус |
|---|---|---|
| 40 | Поддержка именованных провайдеров: `IAiProviderFactory.GetProvider("openai")` | 🔴 |
| 41 | `AddAiProvider("openai", config => ...)` — регистрация нескольких провайдеров | 🔴 |
| 42 | `FallbackProvider` — автоматический переход на резервный провайдер при ошибке | 🔴 |

### 4.5 Дополнительные возможности провайдеров

| # | Задача | Статус |
|---|---|---|
| 43 | `OpenAiProvider` — поддержка tool calls / function calling | 🔴 |
| 44 | `GoogleGenAiProvider` — поддержка `ThinkingConfig` (thinking budget) | 🔴 |
| 45 | `NativeSdkTransport` — поддержка поля `fallbacks` (список резервных моделей OpenRouter) | 🔴 |
| 46 | `NativeSdkTransport` — поддержка `provider.order` для явного роутинга | 🔴 |

---

## 📦 Phase 5 — Distribution (низкий приоритет)

| # | Задача | Статус |
|---|---|---|
| 47 | Настроить `Directory.Build.props` с общими свойствами (версия, авторы, лицензия) | 🔴 |
| 48 | Настроить упаковку NuGet для каждого проекта (`<PackageId>`, `<Description>`) | 🔴 |
| 49 | Добавить `README.md` в корень каждого проекта для NuGet | 🔴 |
| 50 | Настроить GitHub Actions CI: build + test + publish to NuGet | 🔴 |

---

## Легенда приоритетов

| Фаза | Когда делать |
|---|---|
| Phase 1 | Перед первым использованием в продукте |
| Phase 2 | Параллельно с Phase 1 |
| Phase 3 | До выхода в production |
| Phase 4 | По мере необходимости |
| Phase 5 | При публичном релизе библиотеки |
