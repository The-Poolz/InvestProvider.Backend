using MediatR;
using Newtonsoft.Json;
using Net.Web3.EthereumWallet;
using InvestProvider.Backend.Services.Handlers.Contexts;

namespace InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

[method: JsonConstructor]
public class GenerateSignatureRequest(string projectId, EthereumAddress userAddress, string weiAmount) :
    IRequest<GenerateSignatureResponse>,
    IPhaseRequest
{
    [JsonRequired]
    public string ProjectId { get; } = projectId;

    [JsonRequired]
    public EthereumAddress UserAddress { get; } = userAddress;

    [JsonRequired]
    public string WeiAmount { get; } = weiAmount;

    [JsonIgnore]
    public bool FilterPhases => true;

    [JsonIgnore]
    public string? PhaseId => Context.StrapiProjectInfo?.CurrentPhase?.Id;

    [JsonIgnore]
    public long? ChainId => Context.StrapiProjectInfo?.ChainId;

    [JsonIgnore]
    public PhaseContext Context { get; set; } = null!;
}