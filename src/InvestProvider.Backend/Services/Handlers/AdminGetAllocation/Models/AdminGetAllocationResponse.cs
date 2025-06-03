using InvestProvider.Backend.Services.DynamoDb.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminGetAllocation.Models;

public class AdminGetAllocationResponse(IReadOnlyDictionary<string, IReadOnlyCollection<UserData>> data)
{
    public IReadOnlyDictionary<string, IReadOnlyCollection<UserData>> UserData { get; set; } = data;
}