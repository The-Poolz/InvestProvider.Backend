using Net.Web3.EthereumWallet;
using InvestProvider.Backend.Services.Strapi.Models;
using InvestProvider.Backend.Services.DynamoDb.Models;

namespace InvestProvider.Backend.Services.Validators.Models;

public interface IWhiteListSignature
{
    public string ProjectId { get; }
    public EthereumAddress UserAddress { get; }
    public decimal Amount { get; }
    public decimal InvestedAmount { get; }
    public ProjectInfo StrapiProjectInfo { get; }
    public WhiteList WhiteList { get; set; }
}