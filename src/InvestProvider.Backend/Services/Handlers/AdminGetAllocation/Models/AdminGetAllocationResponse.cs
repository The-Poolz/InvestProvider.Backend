using InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminGetAllocation.Models;

public class AdminGetAllocationResponse(string phaseId, IReadOnlyCollection<UserWithAmount> whiteList)
{
    public string PhaseId { get; } = phaseId;
    public IReadOnlyCollection<UserWithAmount> WhiteList { get; } = whiteList;
}