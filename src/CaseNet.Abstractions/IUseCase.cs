using System.Threading;
using System.Threading.Tasks;

namespace CaseNet.Abstractions
{
    public interface IUseCase<in TRequest, TResponse>
    {
        Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken);
    }
}