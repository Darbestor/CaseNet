using System.Threading;
using System.Threading.Tasks;

namespace CaseNet.Abstractions
{
    public interface IUseCaseBehavior<TRequest, TResponse>
    {
        Task<TResponse> HandleAsync(
            TRequest request,
            InteractorContext<TRequest, TResponse> context,
            CancellationToken ct);
    }
}