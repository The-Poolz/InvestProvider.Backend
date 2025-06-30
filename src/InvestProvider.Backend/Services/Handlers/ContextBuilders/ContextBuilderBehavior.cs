using MediatR;

namespace InvestProvider.Backend.Services.Handlers.ContextBuilders;

public class ContextBuilderBehavior<TRequest, TResponse>(IRequestContextBuilder<TRequest>? builder)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (builder is not null)
            await builder.BuildAsync(request, cancellationToken);

        return await next();
    }
}
