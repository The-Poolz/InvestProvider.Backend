using Net.Web3.EthereumWallet;

namespace InvestProvider.Backend.Services.Web3.Contracts;

public interface ILockDealNFTContract
{
    public EthereumAddress TokenOf(long chainId, long poolId);
}