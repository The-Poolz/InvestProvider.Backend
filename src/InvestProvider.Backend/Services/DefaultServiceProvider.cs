using Amazon.DynamoDBv2;
using Net.Cache.DynamoDb.ERC20;
using Amazon.DynamoDBv2.DataModel;
using NethereumGenerators.Interfaces;
using InvestProvider.Backend.Extensions;
using InvestProvider.Backend.Services.Web3;
using InvestProvider.Backend.Services.Strapi;
using Microsoft.Extensions.DependencyInjection;
using poolz.finance.csharp.contracts.LockDealNFT;
using InvestProvider.Backend.Services.Web3.Eip712;
using poolz.finance.csharp.contracts.InvestProvider;
using InvestProvider.Backend.Services.Web3.Contracts;

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
            .AddScoped<SecretsManager.SecretManager>()
#endif
            .AddHandlers()
            .AddScoped<ISignatureGenerator, SignatureGenerator>()
            .AddScoped<IRpcProvider, ChainProvider>()
            .AddScoped<IChainProvider<ContractType>, ChainProvider>()
            .AddScoped<IInvestProviderService<ContractType>, InvestProviderService<ContractType>>()
            .AddScoped<ILockDealNFTService<ContractType>, LockDealNFTService<ContractType>>()
            .AddScoped<IStrapiClient, StrapiClient>()
            .AddScoped<ERC20CacheProvider>()
            .AddScoped<IDynamoDBContext, DynamoDBContext>()
            .AddScoped<IAmazonDynamoDB, AmazonDynamoDBClient>();
        return services.BuildServiceProvider();
    }
}