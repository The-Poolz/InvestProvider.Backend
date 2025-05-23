using Net.Web3.EthereumWallet;
using InvestProvider.Backend.Services.Web3.Contracts.Models;

namespace InvestProvider.Backend.Services.Web3.Contracts;

public class LockDealNFTContract(IChainProvider chainProvider) : ILockDealNFTContract
{
    public EthereumAddress TokenOf(long chainId, long poolId)
    {
        var web3 = chainProvider.Web3(chainId);
        var contractAddress = chainProvider.LockDealNFTContract(chainId);

        var handler = web3.Eth.GetContractHandler(contractAddress);
        var query = new TokenOfRequest(poolId);

        return handler.QueryAsync<TokenOfRequest, string>(query)
            .GetAwaiter()
            .GetResult();
    }
}