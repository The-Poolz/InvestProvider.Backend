using MediatR;
using Newtonsoft.Json;
using Poolz.Finance.CSharp.Strapi;
using InvestProvider.Backend.Services.Handlers.Contexts;
using Net.Web3.EthereumWallet;

namespace InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;

public class AdminWriteAllocationRequest(string projectId, string phaseId, ICollection<UserWithAmount> users) :
    IRequest<AdminWriteAllocationResponse>,
    IPhaseRequest
{
    [JsonRequired]
    public string ProjectId { get; } = projectId;

    [JsonRequired]
    public string PhaseId { get; } = phaseId;

    [JsonRequired]
    public ICollection<UserWithAmount> Users { get; set; } = users;

    [JsonIgnore]
    public bool FilterPhases => false;

    [JsonIgnore]
    public long? ChainId => Context.StrapiProjectInfo?.ChainId;

    [JsonIgnore]
    public EthereumAddress? UserAddress => null;

    [JsonIgnore]
    public string? WeiAmount => null;

    [JsonIgnore]
    public PhaseContext Context { get; set; } = null!;
}