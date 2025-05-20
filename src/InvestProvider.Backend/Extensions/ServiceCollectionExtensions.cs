using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace InvestProvider.Backend.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHandlers(this IServiceCollection serviceCollection) =>
        serviceCollection.AddMediatR(x => x.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
}