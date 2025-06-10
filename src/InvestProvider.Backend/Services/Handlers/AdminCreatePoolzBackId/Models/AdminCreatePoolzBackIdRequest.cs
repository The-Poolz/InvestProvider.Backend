using MediatR;
using Newtonsoft.Json;
using InvestProvider.Backend.Services.DynamoDb.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId.Models;

public class AdminCreatePoolzBackIdRequest : ProjectsInformation, IRequest<AdminCreatePoolzBackIdResponse>
{
    [JsonRequired]
    public long ChainId { get; set; }
}