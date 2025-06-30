using MediatR;
using Newtonsoft.Json;
using Amazon.DynamoDBv2.DataModel;
using InvestProvider.Backend.Services.Strapi.Models;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Handlers.Contexts;

namespace InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId.Models;

public class AdminCreatePoolzBackIdRequest :
    ProjectsInformation,
    IRequest<AdminCreatePoolzBackIdResponse>,
    IExistActivePhase,
    IInvestPool
{
    [JsonRequired]
    [DynamoDBIgnore]
    public long ChainId { get; set; }

    [JsonIgnore]
    [DynamoDBIgnore]
    public bool FilterPhases => false;

    [JsonIgnore]
    [DynamoDBIgnore]
    public ProjectInfo StrapiProjectInfo { get; set; } = null!;

    [JsonIgnore]
    [DynamoDBIgnore]
    public string PhaseId => StrapiProjectInfo.CurrentPhase!.Id;
}