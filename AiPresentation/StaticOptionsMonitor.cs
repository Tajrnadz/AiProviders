using Microsoft.Extensions.Options;

namespace AiPresentation;

/// <summary>
/// Простая реализация IOptionsMonitor&lt;T&gt;, возвращающая заранее созданный экземпляр.
/// Используется для создания провайдеров с конфигурацией, отличной от основной секции DI.
/// </summary>
internal sealed class StaticOptionsMonitor<T>(T value) : IOptionsMonitor<T>
{
    public T CurrentValue { get; } = value;

    public T Get(string? name) => CurrentValue;

    public IDisposable? OnChange(Action<T, string?> listener) => null;
}
