using MediatR;
using Newtonsoft.Json;
using Amazon.DynamoDBv2.DataModel;
using InvestProvider.Backend.Services.Validators;
using InvestProvider.Backend.Services.DynamoDb.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId.Models;

public class AdminCreatePoolzBackIdRequest :
    ProjectsInformation,
    IRequest<AdminCreatePoolzBackIdResponse>
{
    [JsonRequired]
    [DynamoDBIgnore]
    public long ChainId { get; set; }

    [JsonIgnore]
    public bool FilterPhases => false;

    [JsonIgnore]
    public PhaseValidationContext PhaseContext { get; } = new();

    [JsonIgnore]
    public string PhaseId => PhaseContext.StrapiProjectInfo.CurrentPhase!.Id;
}