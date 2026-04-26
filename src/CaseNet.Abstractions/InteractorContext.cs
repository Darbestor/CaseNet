using System.Threading;
using System.Threading.Tasks;

namespace CaseNet.Abstractions
{
    public sealed class InteractorContext<TRequest, TResponse>
    {
        private readonly IUseCaseBehavior<TRequest, TResponse>[] _behaviors;
        private readonly IUseCase<TRequest, TResponse> _inner;
        private int _index;

        public InteractorContext(
            IUseCase<TRequest, TResponse> inner,
            IUseCaseBehavior<TRequest, TResponse>[] behaviors)
        {
            _inner = inner;
            _behaviors = behaviors;
            _index = 0;
        }

        public Task<TResponse> NextAsync(TRequest request, CancellationToken ct)
        {
            if (_index < _behaviors.Length)
            {
                var behavior = _behaviors[_index++];
                return behavior.HandleAsync(request, this, ct);
            }

            return _inner.ExecuteAsync(request, ct);
        }
    }
}