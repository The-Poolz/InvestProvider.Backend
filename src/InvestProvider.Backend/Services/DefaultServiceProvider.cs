using InvestProvider.Backend.Extensions;
using InvestProvider.Backend.Services.Web3;
using InvestProvider.Backend.Services.Strapi;
using Microsoft.Extensions.DependencyInjection;
using InvestProvider.Backend.Services.Web3.Contracts;

namespace InvestProvider.Backend.Services;

public static class DefaultServiceProvider
{
    public static IServiceProvider Build()
    {
        var services = new ServiceCollection()
            .AddHandlers()
            .AddScoped<IChainProvider, ChainProvider>()
            .AddScoped<IInvestProviderContract, InvestProviderContract>()
            .AddScoped<IStrapiClient, StrapiClient>();
        return services.BuildServiceProvider();
    }
}