# CaseNet

A .NET library that simplifies the Use Case pattern through source generation, automatically creating interfaces and interactors for your use cases.

## Features

- Clean use case abstraction with `IUseCase<TRequest, TResponse>`
- Source generator that automatically creates interfaces and interactor classes
- Dynamic service collection registration via extension methods
- Behavior pipeline support via `IUseCaseBehavior` for cross-cutting concerns
- Seamless integration with ASP.NET Core and dependency injection

## Usage

### Define a Use Case

Implement the `IUseCase<TRequest, TResponse>` interface:

```csharp
public class CreateUser : IUseCase<CreateUserRequest, CreateUserResponse>
{
    public Task<CreateUserResponse> ExecuteAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

The source generator automatically creates:
- An `ICreateUser` interface
- A `CreateUserInteractor` class that implements the interface. This class wraps `CreateUser` with the behavior pipeline

### Register Use Cases

Add use cases to the service collection by calling `services.Add{ProjectName}UseCases()`, where `ProjectName` is your project's assembly name:

```csharp
services.AddMyProjectUseCases();
```

### Execute use case

Inject the generated interface where you need to call use case:

```csharp
public class UserService
{
    public async Task CreateUserAsync(
        CreateUserRequest request,
        ICreateUser useCase,
        CancellationToken cancellationToken)
    {
        var response = await useCase.ExecuteAsync(request, cancellationToken);
    }
}
```

### Add Behaviors

Behaviors implement the pipeline pattern for cross-cutting concerns like logging, validation, or error handling:

```csharp
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
```

Register behaviors as open generics in the service collection. This allows the interactor to discover and apply them:

```csharp
services.AddScoped(typeof(IUseCaseBehavior<,>), typeof(LoggingBehavior<,>));
```