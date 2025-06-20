using MediatR;
using Newtonsoft.Json;
using Poolz.Finance.CSharp.Strapi;
using InvestProvider.Backend.Services.Strapi.Models;
using InvestProvider.Backend.Services.Validators.Models;
using ProjectsInformation = InvestProvider.Backend.Services.DynamoDb.Models.ProjectsInformation;

namespace InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;

public class AdminWriteAllocationRequest :
    IRequest<AdminWriteAllocationResponse>,
    IValidatedDynamoDbProjectInfo,
    IValidatedPhase
{
    [JsonRequired]
    public string ProjectId { get; set; } = null!;

    [JsonRequired]
    public string PhaseId { get; set; } = null!;

    [JsonRequired]
    public ICollection<UserWithAmount> Users { get; set; } = null!;

    [JsonIgnore]
    public bool FilterPhases => false;

    [JsonIgnore]
    public ProjectInfo StrapiProjectInfo { get; set; } = null!;

    [JsonIgnore]
    public ProjectsInformation DynamoDbProjectsInfo { get; set; } = null!;

    [JsonIgnore]
    public ComponentPhaseStartEndAmount Phase { get; set; } = null!;
}