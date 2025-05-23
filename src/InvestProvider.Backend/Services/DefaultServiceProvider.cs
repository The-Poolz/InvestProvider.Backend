using Net.Cache;
using Net.Cache.DynamoDb;
using Net.Cache.DynamoDb.ERC20;
using InvestProvider.Backend.Extensions;
using InvestProvider.Backend.Services.Web3;
using InvestProvider.Backend.Services.Strapi;
using Microsoft.Extensions.DependencyInjection;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Web3.Eip712;

namespace InvestProvider.Backend.Services;

public static class DefaultServiceProvider
{
    public static IServiceProvider Build()
    {
        var services = new ServiceCollection()
#if DEBUG
            .AddScoped<ISignerManager, EnvSignerManager>()
#else
            .AddScoped<ISignerManager, SignerManager>()
#endif
            .AddHandlers()
            .AddScoped<ISignatureGenerator, SignatureGenerator>()
            .AddScoped<IChainProvider, ChainProvider>()
            .AddScoped<IInvestProviderContract, InvestProviderContract>()
            .AddScoped<ILockDealNFTContract, LockDealNFTContract>()
            .AddScoped<IStrapiClient, StrapiClient>()
            .AddScoped<ERC20CacheProvider>()
            .AddScoped<CacheProvider<string, UserData>>(_ => new CacheProvider<string, UserData>(new DynamoDbStorageProvider<string, UserData>()));
        return services.BuildServiceProvider();
    }
}