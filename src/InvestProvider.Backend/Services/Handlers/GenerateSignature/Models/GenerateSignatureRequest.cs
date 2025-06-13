using MediatR;
using Newtonsoft.Json;
using Net.Web3.EthereumWallet;
using InvestProvider.Backend.Services.Strapi.Models;
using InvestProvider.Backend.Services.Validators.Models;

namespace InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

public class GenerateSignatureRequest : IRequest<GenerateSignatureResponse>, IHasProjectInfo
{
    [JsonRequired]
    public string ProjectId { get; set; } = null!;

    [JsonRequired]
    public EthereumAddress UserAddress { get; set; } = null!;

    [JsonRequired]
    public string WeiAmount { get; set; } = null!;

    [JsonIgnore]
    public ProjectInfo? ProjectInfo { get; set; }
}