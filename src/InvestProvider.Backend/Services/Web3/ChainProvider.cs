using Nethereum.Web3;
using Net.Web3.EthereumWallet;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Strapi.Models;

namespace InvestProvider.Backend.Services.Web3;

public class ChainProvider(IStrapiClient strapi) : IChainProvider
{
    private readonly Dictionary<long, OnChainInfo> ChainsInfo = new();

    public IWeb3 Web3(long chainId)
    {
        var chainInfo = FetchChainInfo(chainId);
        return new Nethereum.Web3.Web3(chainInfo.RpcUrl);
    }

    public EthereumAddress InvestedProviderContract(long chainId) => FetchChainInfo(chainId).InvestedProvider;
    public EthereumAddress LockDealNFTContract(long chainId) => FetchChainInfo(chainId).LockDealNFT;

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