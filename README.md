# CaseNet

Use case pattern with source generation for .NET.

## Features

- Clean use case abstraction with `IUseCase<TRequest, TResponse>`
- Source generator for automatic interactor registration
- Behavior pipeline support via `IUseCaseBehavior`
- Works with ASP.NET Core and dependency injection

## Usage

1. Install the CaseNet package
2. Implement `IUseCase<TRequest, TResponse>` in your interactors
3. The source generator automatically registers them in your DI container

## Example

```csharp
public class CreateUserUseCase : IUseCase<CreateUserRequest, CreateUserResponse>
{
    public Task<CreateUserResponse> ExecuteAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```
