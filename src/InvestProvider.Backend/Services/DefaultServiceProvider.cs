using Net.Cache;
using SecretsManager;
using Net.Cache.DynamoDb;
using Net.Cache.DynamoDb.ERC20;
using NethereumGenerators.Interfaces;
using InvestProvider.Backend.Extensions;
using InvestProvider.Backend.Services.Web3;
using InvestProvider.Backend.Services.Strapi;
using Microsoft.Extensions.DependencyInjection;
using poolz.finance.csharp.contracts.LockDealNFT;
using InvestProvider.Backend.Services.Web3.Eip712;
using poolz.finance.csharp.contracts.InvestProvider;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.DynamoDb.Models;

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
            .AddScoped<IRpcProvider, ChainProvider>()
            .AddScoped<IChainProvider<ContractType>, ChainProvider>()
            .AddScoped<IInvestProviderService<ContractType>, InvestProviderService<ContractType>>()
            .AddScoped<ILockDealNFTService<ContractType>, LockDealNFTService<ContractType>>()
            .AddScoped<IStrapiClient, StrapiClient>()
            .AddScoped<ERC20CacheProvider>()
            .AddScoped<SecretManager>()
            .AddScoped<CacheProvider<string, UserData>>(_ => new CacheProvider<string, UserData>(new DynamoDbStorageProvider<string, UserData>()));
        return services.BuildServiceProvider();
    }
}