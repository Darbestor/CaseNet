using Microsoft.CodeAnalysis;

namespace CaseNet.Generator;

internal sealed class InteractorMetadata
{
    public InteractorMetadata(
        string @namespace,
        string useCaseName,
        string interfaceName,
        string interactorName,
        string requestType,
        string responseType,
        bool isValid,
        Location location,
        int useCaseCount)
    {
        Namespace = @namespace;
        UseCaseName = useCaseName;
        InterfaceName = interfaceName;
        InteractorName = interactorName;
        RequestType = requestType;
        ResponseType = responseType;
        IsValid = isValid;
        Location = location;
        UseCaseCount = useCaseCount;
    }

    public string Namespace { get; set; }

    public string UseCaseName { get; }

    public string InterfaceName { get; }

    public string InteractorName { get; }

    public string RequestType { get; }

    public string ResponseType { get; }

    public bool IsValid { get; }

    public Location Location { get; }

    public int UseCaseCount { get; }
}