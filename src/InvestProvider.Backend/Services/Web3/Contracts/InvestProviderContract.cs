using Net.Web3.EthereumWallet;
using InvestProvider.Backend.Services.Web3.Contracts.Models;

namespace InvestProvider.Backend.Services.Web3.Contracts;

public class InvestProviderContract(IChainProvider chainProvider) : IInvestProviderContract
{
    public IEnumerable<UserInvest> GetUserInvests(long chainId, long poolId, EthereumAddress user)
    {
        var web3 = chainProvider.Web3(chainId);
        var contractAddress = chainProvider.InvestedProviderContract(chainId);

        var query = new UserInvestRequest(poolId, user);

        var handler = web3.Eth.GetContractQueryHandler<UserInvestRequest>();

        var response = handler
            .QueryDeserializingToObjectAsync<UserInvestResponse>(query, contractAddress)
            .GetAwaiter()
            .GetResult();

        return response?.Invests ?? [];
    }
}