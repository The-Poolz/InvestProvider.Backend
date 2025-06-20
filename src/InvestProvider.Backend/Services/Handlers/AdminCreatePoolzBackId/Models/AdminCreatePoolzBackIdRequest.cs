using MediatR;
using Newtonsoft.Json;
using Amazon.DynamoDBv2.DataModel;
using InvestProvider.Backend.Services.Strapi.Models;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Validators.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId.Models;

public class AdminCreatePoolzBackIdRequest :
    ProjectsInformation,
    IRequest<AdminCreatePoolzBackIdResponse>,
    IValidatedStrapiProjectInfo,
    IInvestPool
{
    [JsonRequired]
    [DynamoDBIgnore]
    public long ChainId { get; set; }

    [JsonIgnore]
    public bool FilterPhases => false;

    [JsonIgnore]
    public ProjectInfo StrapiProjectInfo { get; set; } = null!;
}