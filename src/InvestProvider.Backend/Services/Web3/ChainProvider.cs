using Nethereum.Web3;
using Nethereum.JsonRpc.Client;
using Poolz.Finance.CSharp.Http;
using System.Collections.Concurrent;
using EnvironmentManager.Extensions;
using NethereumGenerators.Interfaces;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Strapi.Models;
using InvestProvider.Backend.Services.Web3.Contracts;

namespace InvestProvider.Backend.Services.Web3;

public class ChainProvider(IStrapiClient strapi, IHttpClientFactory httpClientFactory) : IChainProvider<ContractType>
{
    private readonly ConcurrentDictionary<long, Lazy<OnChainInfo>> ChainsInfo = new();

    public static string RpcUrl(long chainId)
    {
        return $"{Env.BASE_URL_OF_RPC.GetRequired()}{chainId}";
    }

    public IWeb3 Web3(long chainId)
    {
        var rpcUrl = RpcUrl(chainId);
        var httpClient = httpClientFactory.Create(rpcUrl);
        return new Nethereum.Web3.Web3(new RpcClient(new Uri(rpcUrl), httpClient));
    }

    public string ContractAddress(long chainId, ContractType contractType)
    {
        return contractType switch
        {
            ContractType.InvestedProvider => FetchChainInfo(chainId).InvestedProvider,
            ContractType.LockDealNFT => FetchChainInfo(chainId).LockDealNFT,
            _ => throw Error.CONTRACT_TYPE_NOT_SUPPORTED.ToException(new
            {
                ContractType = contractType
            })
        };
    }

    private OnChainInfo FetchChainInfo(long chainId) => ChainsInfo
            .GetOrAdd(
                chainId,
                x => new Lazy<OnChainInfo>(
                    () => strapi.ReceiveOnChainInfoAsync(x).GetAwaiter().GetResult(),
                    LazyThreadSafetyMode.ExecutionAndPublication
                )
            ).Value;
}