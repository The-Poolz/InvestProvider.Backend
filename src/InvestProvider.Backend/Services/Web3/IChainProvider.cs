using Nethereum.Web3;
using Net.Web3.EthereumWallet;

namespace InvestProvider.Backend.Services.Web3;

public interface IChainProvider
{
    public string RpcUrl(long chainId);
    public IWeb3 Web3(long chainId);

    public EthereumAddress InvestedProviderContract(long chainId);
    public EthereumAddress LockDealNFTContract(long chainId);
}