using Net.Web3.EthereumWallet;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Strapi.Models;

namespace InvestProvider.Backend.Services.Validators.Models;

public interface INotAlreadyInvestedAmount
{
    public EthereumAddress UserAddress { get; }
    public decimal Amount { get; set; }
    public byte TokenDecimals { get; set; }
    public ProjectInfo StrapiProjectInfo { get; set; }
    public ProjectsInformation DynamoDbProjectsInfo { get; set; }
}