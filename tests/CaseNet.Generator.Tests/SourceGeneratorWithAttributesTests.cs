using System.Linq;
using System.Reflection;
using CaseNet.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace CaseNet.Generator.Tests;

public class CaseNetGeneratorTests
{
    private static MetadataReference GetAbstractionsReference()
        => MetadataReference.CreateFromFile(typeof(IUseCase<,>).Assembly.Location);

    [Fact]
    public void GenerateUseCaseInterfaceAndInteractor()
    {
        var useCaseClassText = @"
using CaseNet.Abstractions;

namespace CaseNet.Example;

public class GetTodoList
{
    public int[] Ids { get; }
    public GetTodoList(int[] ids) => Ids = ids;
}

public sealed class GetTodoListUseCase : IUseCase<GetTodoList, IReadOnlyCollection<Todo>>
{
    public Task<IReadOnlyCollection<Todo>> ExecuteAsync(GetTodoList request, CancellationToken ct)
        => throw new NotImplementedException();
}";

        var generator = new CaseNetGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(nameof(CaseNetGeneratorTests),
            new[] { CSharpSyntaxTree.ParseText(useCaseClassText) },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                GetAbstractionsReference(),
                MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(CaseNetGeneratorTests))!.Location)
            });

        var runResult = driver.RunGenerators(compilation).GetRunResult();
        var generatedFiles = runResult.GeneratedTrees.ToList();

        Assert.NotEmpty(generatedFiles);

        var interactorFile = generatedFiles.FirstOrDefault(t => t.FilePath.Contains("GetTodoListInteractor"));
        Assert.NotNull(interactorFile);

        var generatedText = interactorFile.GetText().ToString();
        Assert.Contains("IGetTodoListUseCase", generatedText);
        Assert.Contains("GetTodoListInteractor", generatedText);
    }

    [Fact]
    public void DoesNotGenerateForNonUseCaseClass()
    {
        var nonUseCaseClassText = @"
namespace CaseNet.Example;

public class NormalClass
{
    public string Name { get; set; }
}";

        var generator = new CaseNetGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(nameof(DoesNotGenerateForNonUseCaseClass),
            new[] { CSharpSyntaxTree.ParseText(nonUseCaseClassText) },
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        // Our generator should not generate interactor files for non-use case classes
        var interactorFiles = runResult.GeneratedTrees
            .Where(t => t.FilePath.Contains("Interactor.g.cs"))
            .ToList();

        Assert.Empty(interactorFiles);
    }

    [Fact]
    public void GeneratesCorrectInterfaceMethodSignature()
    {
        var useCaseClassText = @"
using CaseNet.Abstractions;
using System.Collections.Generic;

namespace MyApp;

public class CreateItem
{
    public string Name { get; set; }
}

public class CreateItemUseCase : IUseCase<CreateItem, ItemResult>
{
    public Task<ItemResult> ExecuteAsync(CreateItem request, CancellationToken ct)
        => Task.FromResult(new ItemResult());
}

public class ItemResult { }";

        var generator = new CaseNetGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(nameof(GeneratesCorrectInterfaceMethodSignature),
            new[] { CSharpSyntaxTree.ParseText(useCaseClassText) },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                GetAbstractionsReference()
            });

        var runResult = driver.RunGenerators(compilation).GetRunResult();
        var generatedFile = runResult.GeneratedTrees
            .First(t => t.FilePath.Contains("CreateItemInteractor"));

        var generatedText = generatedFile.GetText().ToString();

        Assert.Contains("ICreateItemUseCase", generatedText);
        Assert.Contains("ExecuteAsync", generatedText);
        Assert.Contains("CreateItem request", generatedText);
        Assert.Contains("CancellationToken ct", generatedText);
    }

    [Fact]
    public void GeneratesCorrectInteractorClassStructure()
    {
        var useCaseClassText = @"
using CaseNet.Abstractions;

namespace CaseNet.Example;

public class GetTodoList
{
    public int[] Ids { get; }
    public GetTodoList(int[] ids) => Ids = ids;
}

public sealed class GetTodoListUseCase : IUseCase<GetTodoList, IReadOnlyCollection<Todo>>
{
    public Task<IReadOnlyCollection<Todo>> ExecuteAsync(GetTodoList request, CancellationToken ct)
        => throw new NotImplementedException();
}";

        var generator = new CaseNetGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(nameof(GeneratesCorrectInteractorClassStructure),
            new[] { CSharpSyntaxTree.ParseText(useCaseClassText) },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                GetAbstractionsReference()
            });

        var runResult = driver.RunGenerators(compilation).GetRunResult();
        var generatedFile = runResult.GeneratedTrees
            .First(t => t.FilePath.Contains("GetTodoListInteractor"));

        var generatedText = generatedFile.GetText().ToString();

        Assert.Contains("sealed class GetTodoListInteractor", generatedText);
        Assert.Contains("IGetTodoListUseCase", generatedText);
        Assert.Contains("GetTodoListUseCase _inner", generatedText);
        Assert.Contains("_behaviors", generatedText);
        Assert.Contains("GetTodoListInteractor(", generatedText);
        Assert.Contains("InteractorContext<", generatedText);
    }

    [Fact]
    public void GeneratesForNonUseCaseNamedImplementor()
    {
        var useCaseClassText = @"
using CaseNet.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace MyApp;

public class ProcessOrder
{
    public int OrderId { get; set; }
}

public class ProcessOrderHandler : IUseCase<ProcessOrder, bool>
{
    public Task<bool> ExecuteAsync(ProcessOrder request, CancellationToken ct)
        => Task.FromResult(true);
}";

        var generator = new CaseNetGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(nameof(GeneratesForNonUseCaseNamedImplementor),
            new[] { CSharpSyntaxTree.ParseText(useCaseClassText) },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                GetAbstractionsReference()
            });

        var runResult = driver.RunGenerators(compilation).GetRunResult();
        var generatedFile = runResult.GeneratedTrees
            .First(t => t.FilePath.Contains("ProcessOrderHandlerInteractor"));

        var generatedText = generatedFile.GetText().ToString();

        Assert.Contains("IProcessOrderHandler", generatedText);
        Assert.Contains("ProcessOrderHandlerInteractor", generatedText);
        Assert.Contains("ProcessOrderHandler _inner", generatedText);
    }

    [Fact]
    public void SkipsAbstractUseCaseClass()
    {
        var useCaseClassText = @"
using CaseNet.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace MyApp;

public abstract class AbstractUseCase : IUseCase<string, string>
{
    public abstract Task<string> ExecuteAsync(string request, CancellationToken ct);
}";

        var generator = new CaseNetGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(nameof(SkipsAbstractUseCaseClass),
            new[] { CSharpSyntaxTree.ParseText(useCaseClassText) },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                GetAbstractionsReference()
            });

        var runResult = driver.RunGenerators(compilation).GetRunResult();
        var interactorFiles = runResult.GeneratedTrees
            .Where(t => t.FilePath.Contains("Interactor.g.cs"))
            .ToList();

        Assert.Empty(interactorFiles);
        var diag = runResult.Diagnostics.FirstOrDefault(d => d.Id == "CN002");
        Assert.NotNull(diag);
    }

    [Fact]
    public void ReportsErrorForMultipleIUseCaseImplementations()
    {
        var useCaseClassText = @"
using CaseNet.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace MyApp;

public class MultiUseCase : 
    IUseCase<string, string>, 
    IUseCase<int, int>
{
    public Task<string> ExecuteAsync(string request, CancellationToken ct)
        => Task.FromResult(request);
    public Task<int> ExecuteAsync(int request, CancellationToken ct)
        => Task.FromResult(request);
}";

        var generator = new CaseNetGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(nameof(ReportsErrorForMultipleIUseCaseImplementations),
            new[] { CSharpSyntaxTree.ParseText(useCaseClassText) },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                GetAbstractionsReference()
            });

        var runResult = driver.RunGenerators(compilation).GetRunResult();
        var interactorFiles = runResult.GeneratedTrees
            .Where(t => t.FilePath.Contains("Interactor.g.cs"))
            .ToList();

        Assert.Empty(interactorFiles);
        var diag = runResult.Diagnostics.FirstOrDefault(d => d.Id == "CN001");
        Assert.NotNull(diag);
    }

    [Fact]
    public void GeneratesDiRegistrationFile()
    {
        var useCaseClassText = @"
using CaseNet.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace MyApp;

public class CreateItem
{
    public string Name { get; set; }
}

public class CreateItemUseCase : IUseCase<CreateItem, ItemResult>
{
    public Task<ItemResult> ExecuteAsync(CreateItem request, CancellationToken ct)
        => Task.FromResult(new ItemResult());
}

public class ItemResult { }";

        var generator = new CaseNetGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(nameof(GeneratesDiRegistrationFile),
            new[] { CSharpSyntaxTree.ParseText(useCaseClassText) },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                GetAbstractionsReference()
            });

        var runResult = driver.RunGenerators(compilation).GetRunResult();
        var diFile = runResult.GeneratedTrees
            .FirstOrDefault(t => t.FilePath.Contains("GeneratedUseCaseRegistrations.g.cs"));

        Assert.NotNull(diFile);
        var diText = diFile.GetText().ToString();
        Assert.Contains("AddGeneratedUseCases", diText);
        Assert.Contains("MyApp.ICreateItemUseCase", diText);
        Assert.Contains("MyApp.CreateItemInteractor", diText);
    }
}