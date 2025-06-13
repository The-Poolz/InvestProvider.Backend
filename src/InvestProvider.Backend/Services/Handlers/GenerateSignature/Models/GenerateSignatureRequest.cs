using MediatR;
using Newtonsoft.Json;
using Net.Web3.EthereumWallet;
using InvestProvider.Backend.Services.Strapi.Models;
using InvestProvider.Backend.Services.DynamoDb.Models;

namespace InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

public class GenerateSignatureRequest : IRequest<GenerateSignatureResponse>, IValidatedGenerateSignatureRequest
{
    [JsonRequired]
    public string ProjectId { get; set; } = null!;

    [JsonRequired]
    public EthereumAddress UserAddress { get; set; } = null!;

    [JsonRequired]
    public string WeiAmount { get; set; } = null!;

    [JsonIgnore]
    public ProjectInfo StrapiProjectInfo { get; set; } = null!;

    [JsonIgnore]
    public ProjectsInformation DynamoDbProjectsInfo { get; set; } = null!;
}