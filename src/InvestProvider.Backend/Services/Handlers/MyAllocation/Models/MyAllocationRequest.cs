using MediatR;
using Newtonsoft.Json;
using Net.Web3.EthereumWallet;
using Poolz.Finance.CSharp.Strapi;
using InvestProvider.Backend.Services.Handlers.Contexts;

namespace InvestProvider.Backend.Services.Handlers.MyAllocation.Models;

[method: JsonConstructor]
public class MyAllocationRequest(string projectId, EthereumAddress userAddress) :
    IRequest<MyAllocationResponse>,
    IPhaseRequest
{
    [JsonRequired]
    public string ProjectId { get; } = projectId;

    [JsonRequired]
    public EthereumAddress UserAddress { get; } = userAddress;

    [JsonIgnore]
    public bool FilterPhases => true;

    [JsonIgnore]
    public string? PhaseId => Context.StrapiProjectInfo?.CurrentPhase?.Id;

    [JsonIgnore]
    public string? WeiAmount => null;

    [JsonIgnore]
    public long? ChainId => Context.StrapiProjectInfo?.ChainId;

    [JsonIgnore]
    public PhaseContext Context { get; set; } = null!;
}