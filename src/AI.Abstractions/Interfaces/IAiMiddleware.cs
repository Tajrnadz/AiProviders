using AI.Abstractions.Models;

namespace AI.Abstractions.Interfaces;

public interface IAiMiddleware
{
    Task<AiResponse> InvokeAsync(
        AiRequest request,
        Func<AiRequest, CancellationToken, Task<AiResponse>> next,
        CancellationToken cancellationToken = default);
}
