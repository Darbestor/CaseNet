using CaseNet.Example;
using CaseNet.Example.Generated;

var builder = WebApplication.CreateSlimBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddUseCases();
builder.Services.AddLoggingBehaviour();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", (IGetTodoListUseCase useCase, CancellationToken cancellationToken)
        =>
    {
        return TypedResults.Ok(useCase.ExecuteAsync(new GetTodoList([1, 2, 3]), cancellationToken));
    })
    .WithName("GetTodos");

app.Run();

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);