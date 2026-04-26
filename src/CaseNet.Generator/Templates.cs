using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Scriban;

namespace CaseNet.Generator;

public static class Templates
{
    private static readonly Template UseCaseTemplate;
    private static readonly Template DiRegistrationTemplate;

    static Templates()
    {
        var assembly = Assembly.GetExecutingAssembly();

        UseCaseTemplate = LoadTemplate(assembly, "CaseNet.Generator.Templates.UseCaseTemplate.sbntxt");
        DiRegistrationTemplate = LoadTemplate(assembly, "CaseNet.Generator.Templates.DiRegistrationTemplate.sbntxt");
    }

    private static Template LoadTemplate(Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName)
                           ?? throw new InvalidOperationException($"Could not find embedded resource '{resourceName}'");
        using var reader = new StreamReader(stream);
        var templateContent = reader.ReadToEnd();
        return Template.Parse(templateContent);
    }

    public static string RenderUseCaseTemplate(
        string @namespace,
        string interfaceName,
        string interactorName,
        string useCaseName,
        string requestType,
        string responseType)
    {
        var model = new
        {
            @namespace,
            interface_name = interfaceName,
            interactor_name = interactorName,
            use_case_name = useCaseName,
            request_type = requestType,
            response_type = responseType
        };
        return UseCaseTemplate.Render(model);
    }

    public static string RenderDiRegistrationTemplate(
        string @namespace,
        string assemblyName,
        IReadOnlyCollection<string> interactorNames)
    {
        var model = new
        {
            @namespace,
            assembly_name = assemblyName,
            interactors = interactorNames
        };
        return DiRegistrationTemplate.Render(model);
    }
}