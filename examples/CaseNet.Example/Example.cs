using Immediate.Handlers.Shared;

namespace CaseNet.Example;

[Handler]
public sealed partial class TestHandler
{
    private ValueTask<int> HandleAsync(GetTodoList query, CancellationToken token) => default;
}