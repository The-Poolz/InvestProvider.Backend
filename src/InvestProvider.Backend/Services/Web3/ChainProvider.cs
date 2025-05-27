using Nethereum.Web3;
using NethereumGenerators.Interfaces;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Strapi.Models;
using InvestProvider.Backend.Services.Web3.Contracts;

namespace InvestProvider.Backend.Services.Web3;

public class ChainProvider(IStrapiClient strapi) : IChainProvider<ContractType>, IRpcProvider
{
    private readonly Dictionary<long, OnChainInfo> ChainsInfo = new();

    public string RpcUrl(long chainId)
    {
        return FetchChainInfo(chainId).RpcUrl;
    }

    public IWeb3 Web3(long chainId)
    {
        return new Nethereum.Web3.Web3(RpcUrl(chainId));
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

    private OnChainInfo FetchChainInfo(long chainId)
    {
        if (ChainsInfo.TryGetValue(chainId, out var chain))
        {
            return chain;
        }

        var chainInfo = strapi.ReceiveOnChainInfo(chainId);
        ChainsInfo.Add(chainId, chainInfo);
        return chainInfo;
    }
}