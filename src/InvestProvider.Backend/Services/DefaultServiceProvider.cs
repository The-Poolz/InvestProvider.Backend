using Amazon.DynamoDBv2;
using Net.Cache.DynamoDb.ERC20;
using Amazon.DynamoDBv2.DataModel;
using NethereumGenerators.Interfaces;
using InvestProvider.Backend.Services.Web3;
using InvestProvider.Backend.Services.Strapi;
using Microsoft.Extensions.DependencyInjection;
using poolz.finance.csharp.contracts.LockDealNFT;
using InvestProvider.Backend.Services.Web3.Eip712;
using poolz.finance.csharp.contracts.InvestProvider;
using InvestProvider.Backend.Services.Web3.Contracts;
using System.Reflection;
using MediatR;
using InvestProvider.Backend.Services.Handlers.ContextBuilders;
using MediatR.Extensions.FluentValidation.AspNetCore;

namespace InvestProvider.Backend.Services;

public static class DefaultServiceProvider
{
    public static IServiceProvider Build() => new ServiceCollection()
#if DEBUG
        .AddScoped<ISignerManager, EnvSignerManager>()
#else
        .AddScoped<ISignerManager, SignerManager>()
        .AddScoped<SecretsManager.SecretManager>()
#endif
        .AddMediatR(x => x.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))
        .AddFluentValidation([Assembly.GetExecutingAssembly()])
        .AddTransient(typeof(IPipelineBehavior<,>), typeof(ContextBuilderBehavior<,>))
        .AddTransient(typeof(IRequestContextBuilder<>), typeof(PhaseContextBuilder<>))
        .AddSingleton<IRpcProvider, ChainProvider>()
        .AddSingleton<IChainProvider<ContractType>, ChainProvider>()
        .AddSingleton<IStrapiClient, StrapiClient>()
        .AddScoped<ISignatureGenerator, SignatureGenerator>()
        .AddScoped<IInvestProviderService<ContractType>, InvestProviderService<ContractType>>()
        .AddScoped<ILockDealNFTService<ContractType>, LockDealNFTService<ContractType>>()
        .AddScoped<ERC20CacheProvider>()
        .AddScoped<IDynamoDBContext, DynamoDBContext>()
        .AddScoped<IAmazonDynamoDB, AmazonDynamoDBClient>()
        .BuildServiceProvider();
}