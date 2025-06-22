using MediatR;
using Newtonsoft.Json;
using Net.Web3.EthereumWallet;
using InvestProvider.Backend.Services.Validators;

namespace InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

[method: JsonConstructor]
public class GenerateSignatureRequest(string projectId, EthereumAddress userAddress, string weiAmount) :
    IRequest<GenerateSignatureResponse>,
    IUserPhaseRequest
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
    public PhaseValidationContext PhaseContext { get; } = new();

    [JsonIgnore]
    public string PhaseId => PhaseContext.StrapiProjectInfo.CurrentPhase!.Id;

    [JsonIgnore]
    public long ChainId => PhaseContext.StrapiProjectInfo.ChainId;
}
