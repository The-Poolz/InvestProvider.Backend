using FluentValidation;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MediatR.Extensions.FluentValidation.AspNetCore;

namespace InvestProvider.Backend.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHandlers(this IServiceCollection serviceCollection) => serviceCollection
        .AddMediatR(x => x.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

    public static IServiceCollection AddValidators(this IServiceCollection serviceCollection) => serviceCollection
        .AddMediatrRequestValidators();

    private static IServiceCollection AddMediatrRequestValidators(this IServiceCollection serviceCollection) =>
        serviceCollection.AddFluentValidation([Assembly.GetExecutingAssembly()]);

}