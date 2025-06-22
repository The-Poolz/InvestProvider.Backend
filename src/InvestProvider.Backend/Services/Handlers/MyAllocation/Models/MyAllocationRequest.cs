using MediatR;
using Newtonsoft.Json;
using Net.Web3.EthereumWallet;
using InvestProvider.Backend.Services.Validators;

namespace InvestProvider.Backend.Services.Handlers.MyAllocation.Models;

[method: JsonConstructor]
public class MyAllocationRequest(string projectId, EthereumAddress userAddress) :
    IRequest<MyAllocationResponse>
{
    [JsonRequired]
    public string ProjectId { get; } = projectId;

    [JsonRequired]
    public EthereumAddress UserAddress { get; } = userAddress;

    [JsonIgnore]
    public bool FilterPhases => false;

    [JsonIgnore]
    public string PhaseId => PhaseContext.StrapiProjectInfo.CurrentPhase!.Id;

    [JsonIgnore]
    public PhaseValidationContext PhaseContext { get; } = new();

    [JsonIgnore]
    public long ChainId => PhaseContext.StrapiProjectInfo.ChainId;
}