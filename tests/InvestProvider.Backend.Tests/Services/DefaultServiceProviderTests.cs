using Xunit;
using System;
using Net.Cache.DynamoDb.ERC20;
using NethereumGenerators.Interfaces;
using InvestProvider.Backend.Services;
using InvestProvider.Backend.Services.Strapi;
using Microsoft.Extensions.DependencyInjection;
using InvestProvider.Backend.Services.Web3.Contracts;

namespace InvestProvider.Backend.Tests.Services;

public class DefaultServiceProviderTests
{
    [Fact]
    public void Build_ResolvesCoreServices()
    {
        Environment.SetEnvironmentVariable("STRAPI_GRAPHQL_URL", "http://localhost");
        var sp = DefaultServiceProvider.Build();
        Assert.NotNull(sp.GetRequiredService<IChainProvider<ContractType>>());
        Assert.NotNull(sp.GetRequiredService<IStrapiClient>());
        Assert.NotNull(sp.GetRequiredService<IErc20CacheService>());
    }
}
