using Net.Web3.EthereumWallet;

namespace InvestProvider.Backend.Services.Validators.Models;

public interface IHasUserAddress
{
    public EthereumAddress UserAddress { get; }
}
