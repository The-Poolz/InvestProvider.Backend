using MediatR;
using Newtonsoft.Json;
using InvestProvider.Backend.Services.Validators;

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
    public PhaseValidationContext PhaseContext { get; } = new();

    [JsonIgnore]
    public long ChainId => PhaseContext.StrapiProjectInfo.ChainId;
}
