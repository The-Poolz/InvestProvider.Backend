using MediatR;
using Newtonsoft.Json;
using Poolz.Finance.CSharp.Strapi;
using InvestProvider.Backend.Services.Strapi.Models;
using InvestProvider.Backend.Services.Handlers.Contexts;
using ProjectsInformation = InvestProvider.Backend.Services.DynamoDb.Models.ProjectsInformation;

namespace InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;

public class AdminWriteAllocationRequest(string projectId, string phaseId, ICollection<UserWithAmount> users) :
    IRequest<AdminWriteAllocationResponse>,
    IValidatedDynamoDbProjectInfo,
    IExistPhase,
    IWhiteListPhase
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
    public ProjectInfo StrapiProjectInfo { get; set; } = null!;

    [JsonIgnore]
    public long ChainId => StrapiProjectInfo.ChainId;

    [JsonIgnore]
    public ProjectsInformation DynamoDbProjectsInfo { get; set; } = null!;

    [JsonIgnore]
    public ComponentPhaseStartEndAmount Phase { get; set; } = null!;
}