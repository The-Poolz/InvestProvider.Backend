using Net.Web3.EthereumWallet;
using InvestProvider.Backend.Services.Web3.Contracts.Models;

namespace InvestProvider.Backend.Services.Web3.Contracts;

public interface IInvestProviderContract
{
    public IEnumerable<UserInvest> GetUserInvests(long chainId, long poolId, EthereumAddress user);
}