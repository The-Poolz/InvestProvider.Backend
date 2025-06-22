using Xunit;
using InvestProvider.Backend.Services;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.Web3;
using InvestProvider.Backend.Services.Strapi;
using NethereumGenerators.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace InvestProvider.Backend.Tests.Services;

public class DefaultServiceProviderTests
{
    [Fact]
    public void Build_ResolvesCoreServices()
    {
        Environment.SetEnvironmentVariable("STRAPI_GRAPHQL_URL", "http://localhost");
        var sp = DefaultServiceProvider.Build();
        Assert.NotNull(sp.GetRequiredService<IChainProvider<ContractType>>());
        Assert.NotNull(sp.GetRequiredService<IRpcProvider>());
        Assert.NotNull(sp.GetRequiredService<IStrapiClient>());
    }
}
