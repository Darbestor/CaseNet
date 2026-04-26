using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CaseNet.Generator;

internal static class UseCaseParser
{
    internal static InteractorMetadata? Parse(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        var classSymbol = semanticModel.GetDeclaredSymbol(classDecl, ct);
        if (classSymbol is null)
        {
            return null;
        }

        var iUseCaseSymbol = semanticModel.Compilation.GetTypeByMetadataName("CaseNet.Abstractions.IUseCase`2");
        if (iUseCaseSymbol is null)
        {
            return null;
        }

        var implementedIUseCases = classSymbol.AllInterfaces
            .Where(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, iUseCaseSymbol))
            .ToList();

        if (!implementedIUseCases.Any())
        {
            return null;
        }

        var isAbstract = classSymbol.IsAbstract;
        var iUseCaseCount = implementedIUseCases.Count;

        if (isAbstract || iUseCaseCount != 1)
        {
            return null;
        }

        var iUseCaseInterface = implementedIUseCases.First();
        var requestType = iUseCaseInterface.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var responseType = iUseCaseInterface.TypeArguments[1].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var classFullName = classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var ns = $"{classSymbol.ContainingNamespace?.ToDisplayString() ?? "global"}.Generated";
        var interfaceName = $"I{classSymbol.Name}";
        var interactorName = classSymbol.Name.EndsWith("UseCase")
            ? classSymbol.Name.Substring(0, classSymbol.Name.Length - "UseCase".Length) + "Interactor"
            : $"{classSymbol.Name}Interactor";

        return new InteractorMetadata(
            ns,
            classFullName,
            interfaceName,
            interactorName,
            requestType,
            responseType,
            true,
            classSymbol.Locations.First(),
            1);
    }
}