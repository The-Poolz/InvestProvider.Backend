using InvestProvider.Backend.Services.Web3.Contracts.Models;
using Net.Web3.EthereumWallet;

namespace InvestProvider.Backend.Services.Validators.Models;

public interface IHasUserInvestments : IValidatedInvestAmount
{
    public UserInvestments[] UserInvestments { get; set; }
    public decimal InvestedAmount { get; set; }
    public EthereumAddress UserAddress { get; }
}
