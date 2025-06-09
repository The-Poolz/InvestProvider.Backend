using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminGetAllocation.Models;

public class AdminGetAllocationResponse(IReadOnlyDictionary<string, IReadOnlyCollection<UserWithAmount>> whiteList)
{
    public IReadOnlyDictionary<string, IReadOnlyCollection<UserWithAmount>> WhiteList { get; set; } = whiteList;
}