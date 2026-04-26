using CaseNet.Abstractions;

namespace CaseNet.Example;

/// <summary>
///     Example of decorator
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class LoggingBehavior<TRequest, TResponse>
    : IUseCaseBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> HandleAsync(
        TRequest request,
        InteractorContext<TRequest, TResponse> context,
        CancellationToken ct)
    {
        _logger.LogInformation("Handling {Request}", typeof(TRequest).Name);

        var response = await context.NextAsync(request, ct);

        _logger.LogInformation("Handled {Request}", typeof(TRequest).Name);

        return response;
    }
}