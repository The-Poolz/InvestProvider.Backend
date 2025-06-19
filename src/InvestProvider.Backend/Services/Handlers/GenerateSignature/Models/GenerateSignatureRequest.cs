using MediatR;
using Newtonsoft.Json;
using Net.Web3.EthereumWallet;
using InvestProvider.Backend.Services.Strapi.Models;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Validators.Models;
using InvestProvider.Backend.Services.Web3.Contracts.Models;

namespace InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

[method: JsonConstructor]
public class GenerateSignatureRequest(string projectId, EthereumAddress userAddress, string weiAmount) :
    IRequest<GenerateSignatureResponse>, 
    IFcfsSignature,
    IWhiteListSignature
{
    [JsonRequired]
    public string ProjectId { get; } = projectId;

    [JsonRequired]
    public EthereumAddress UserAddress { get; } = userAddress;

    [JsonRequired]
    public string WeiAmount { get; } = weiAmount;

    [JsonIgnore]
    public decimal Amount { get; set; }

    [JsonIgnore]
    public byte TokenDecimals { get; set; }

    [JsonIgnore]
    public UserInvestments[] UserInvestments { get; set; } = null!;

    [JsonIgnore]
    public decimal InvestedAmount { get; set; }

    [JsonIgnore]
    public ProjectInfo StrapiProjectInfo { get; set; } = null!;

    [JsonIgnore]
    public WhiteList WhiteList { get; set; } = null!;

    [JsonIgnore]
    public ProjectsInformation DynamoDbProjectsInfo { get; set; } = null!;
}