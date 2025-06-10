using MediatR;
using Newtonsoft.Json;
using Amazon.DynamoDBv2.DataModel;
using InvestProvider.Backend.Services.DynamoDb.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId.Models;

public class AdminCreatePoolzBackIdRequest : ProjectsInformation, IRequest<AdminCreatePoolzBackIdResponse>
{
    [JsonRequired]
    [DynamoDBIgnore]
    public long ChainId { get; set; }
}