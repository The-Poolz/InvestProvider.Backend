using InvestProvider.Backend.Services.DynamoDb.Models;
using Net.Web3.EthereumWallet;

namespace InvestProvider.Backend.Services.Validators.Models;

public interface IWhiteListUser : IExistActivePhase
{
    public WhiteList WhiteList { get; set; }
    public EthereumAddress UserAddress { get; }
}
