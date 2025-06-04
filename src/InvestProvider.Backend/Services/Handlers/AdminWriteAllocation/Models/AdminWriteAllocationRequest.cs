using MediatR;
using Newtonsoft.Json;
using InvestProvider.Backend.Services.DynamoDb.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;

public class AdminWriteAllocationRequest : IRequest<AdminWriteAllocationResponse>
{
    [JsonRequired]
    public string ProjectId { get; set; } = null!;

    [JsonRequired]
    public string PhaseId { get; set; } = null!;

    [JsonRequired]
    public ICollection<UserWithAmount> Users { get; set; } = null!;

    [JsonIgnore]
    public ICollection<UserData> ToSave => Users.Select(x => new UserData
    {
        PhaseId = PhaseId,
        Amount = x.Amount,
        UserAddress = x.UserAddress
    }).ToArray();
}