using System.Threading;
using System.Threading.Tasks;

namespace InvestProvider.Backend.Services.Handlers.ContextBuilders;

public interface IRequestContextBuilder<in TRequest>
{
    Task BuildAsync(TRequest request, CancellationToken cancellationToken);
}
