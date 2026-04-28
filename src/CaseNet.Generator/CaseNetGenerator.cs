using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CaseNet.Generator;

[Generator(LanguageNames.CSharp)]
public class CaseNetGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor MultipleImplDiagnostic = new(
        "CN001",
        "Multiple IUseCase implementations",
        "Class '{0}' implements multiple IUseCase<,> interfaces, which is not supported",
        "Usage",
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor AbstractClassDiagnostic = new(
        "CN002",
        "Abstract IUseCase implementation",
        "Class '{0}' is abstract and implements IUseCase<,>, skipping generation",
        "Usage",
        DiagnosticSeverity.Warning,
        true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var hasServiceCollection = context
            .MetadataReferencesProvider
            .Where(r => (r?.Display ?? "").Contains("Microsoft.Extensions.DependencyInjection.Abstractions"))
            .Collect()
            .Select((refs, _) => refs.Any())
            .WithTrackingName("MsDi");

        var projectMeta = context.AnalyzerConfigOptionsProvider
            .Select((provider, _) =>
            {
                provider.GlobalOptions.TryGetValue("build_property.AssemblyName", out var assemblyName);
                provider.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);

                return (
                    assemblyName: assemblyName?.Replace(".", string.Empty)
                        .Replace(" ", string.Empty)
                        .Trim() ?? "Unknown",
                    rootNamespace: rootNamespace ?? "Global");
            })
            .WithTrackingName("ProjectInformation");

        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "CaseNetGenerator.run.marker.cs",
            SourceText.From("// CaseNet Generator has run!", Encoding.UTF8)));

        // Individual interactor generation (incremental - only regenerates changed interactors)
        var useCases = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => node is ClassDeclarationSyntax { BaseList: not null },
                static (syntaxContext, ct) => UseCaseParser.Parse(syntaxContext, ct))
            .Where(m => m is not null)
            .Select((m, _) => m!)
            .WithTrackingName("UseCases");

        // For individual interactor generation, combine each interactor with hasMsDi
        var useCaseWithDi = useCases.Combine(hasServiceCollection);

        context.RegisterSourceOutput(useCaseWithDi, (spc, node) =>
        {
            var entry = node.Left;
            var hasDi = node.Right;
            RenderInteractor(spc, entry, hasDi);
        });

        // For DI registration, collect all interactors
        var collectedUseCases = useCases
            .Collect()
            .Combine(hasServiceCollection)
            .Combine(projectMeta)
            .WithTrackingName("ServiceCollectionRegistration");

        context.RegisterSourceOutput(collectedUseCases, (spc, node) =>
        {
            var left = node.Left;
            var interactors = left.Left;
            var hasDi = left.Right;
            var projectInfo = node.Right;

            RenderCollectionServices(
                spc,
                interactors,
                hasDi,
                projectInfo.rootNamespace,
                projectInfo.assemblyName);
        });
    }

    private void RenderCollectionServices(
        SourceProductionContext ctx,
        ImmutableArray<InteractorMetadata> interactors,
        bool hasDi,
        string rootNamespace,
        string assemblyName)
    {
        if (!hasDi)
        {
            return;
        }

        if (interactors.Length == 0)
        {
            return;
        }

        var @namespace = $"{rootNamespace}.Extensions.Generated";
        var diCode = Templates.RenderDiRegistrationTemplate(
            @namespace,
            assemblyName,
            interactors.Select(x => $"global::{x.Namespace}.{x.InteractorName}").ToArray());
        ctx.AddSource("GeneratedUseCaseRegistrations.g.cs", SourceText.From(diCode, Encoding.UTF8));
    }

    private void RenderInteractor(SourceProductionContext ctx, InteractorMetadata entry, bool hasDi)
    {
        if (!hasDi)
        {
            return;
        }

        if (entry.UseCaseCount == 0)
        {
            return;
        }

        if (entry.UseCaseCount > 1)
        {
            ctx.ReportDiagnostic(Diagnostic.Create(MultipleImplDiagnostic, entry.Location, entry.UseCaseName));
            return;
        }

        if (!entry.IsValid)
        {
            ctx.ReportDiagnostic(Diagnostic.Create(AbstractClassDiagnostic, entry.Location, entry.UseCaseName));
            return;
        }

        var generatedCode = Templates.RenderUseCaseTemplate(entry.Namespace!, entry.InterfaceName!,
            entry.InteractorName!, entry.UseCaseName, entry.RequestType!, entry.ResponseType!);

        var fileName = $"{entry.InteractorName}.g.cs";
        ctx.AddSource(fileName, SourceText.From(generatedCode, Encoding.UTF8));
    }
}