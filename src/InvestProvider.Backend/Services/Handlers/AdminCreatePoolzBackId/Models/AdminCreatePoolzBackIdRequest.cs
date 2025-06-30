using MediatR;
using Newtonsoft.Json;
using Amazon.DynamoDBv2.DataModel;
using InvestProvider.Backend.Services.Handlers.Contexts;
using Net.Web3.EthereumWallet;
using ProjectsInformation = InvestProvider.Backend.Services.DynamoDb.Models.ProjectsInformation;

namespace InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId.Models;

public class AdminCreatePoolzBackIdRequest :
    ProjectsInformation,
    IRequest<AdminCreatePoolzBackIdResponse>,
    IPhaseRequest
{
    [JsonRequired]
    [DynamoDBIgnore]
    public long ChainId { get; set; }
    string IPhaseRequest.ProjectId => ProjectId;

    [JsonIgnore]
    [DynamoDBIgnore]
    public bool FilterPhases => false;

    [JsonIgnore]
    [DynamoDBIgnore]
    public EthereumAddress? UserAddress => null;

    [JsonIgnore]
    [DynamoDBIgnore]
    public string? WeiAmount => null;

    [JsonIgnore]
    [DynamoDBIgnore]
    public string? PhaseId => Context.StrapiProjectInfo?.CurrentPhase?.Id;

    [JsonIgnore]
    [DynamoDBIgnore]
    public PhaseContext Context { get; set; } = null!;

    long? IPhaseRequest.ChainId => ChainId;
}