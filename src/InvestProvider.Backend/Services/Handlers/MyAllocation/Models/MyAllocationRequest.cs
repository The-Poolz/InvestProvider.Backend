using MediatR;
using Newtonsoft.Json;
using Net.Web3.EthereumWallet;
using Poolz.Finance.CSharp.Strapi;
using InvestProvider.Backend.Services.Strapi.Models;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Validators.Models;
using ProjectsInformation = InvestProvider.Backend.Services.DynamoDb.Models.ProjectsInformation;

namespace InvestProvider.Backend.Services.Handlers.MyAllocation.Models;

[method: JsonConstructor]
public class MyAllocationRequest(string projectId, EthereumAddress userAddress) :
    IRequest<MyAllocationResponse>,
    IValidatedDynamoDbProjectInfo,
    IExistPhase,
    IWhiteListPhase,
    IWhiteListUser
{
    [JsonRequired]
    public string ProjectId { get; } = projectId;

    [JsonRequired]
    public EthereumAddress UserAddress { get; } = userAddress;

    [JsonIgnore]
    public bool FilterPhases => false;

    [JsonIgnore]
    public string PhaseId => StrapiProjectInfo.CurrentPhase!.Id;

    [JsonIgnore]
    public ProjectInfo StrapiProjectInfo { get; set; } = null!;

    [JsonIgnore]
    public ComponentPhaseStartEndAmount Phase { get; set; } = null!;

    [JsonIgnore]
    public ProjectsInformation DynamoDbProjectsInfo { get; set; } = null!;

    [JsonIgnore]
    public WhiteList WhiteList { get; set; } = null!;

    [JsonIgnore]
    public long ChainId => StrapiProjectInfo.ChainId;
}