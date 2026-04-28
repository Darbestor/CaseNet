using CaseNet.Abstractions;
using CaseNet.Example.Extensions.Generated;

namespace CaseNet.Example;

public static class DependencyInjection
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddCaseNetExampleUseCases();
        return services;
    }

    public static IServiceCollection AddLoggingBehaviour(this IServiceCollection services)
    {
        services.AddScoped(typeof(IUseCaseBehavior<,>), typeof(LoggingBehavior<,>));
        return services;
    }
}