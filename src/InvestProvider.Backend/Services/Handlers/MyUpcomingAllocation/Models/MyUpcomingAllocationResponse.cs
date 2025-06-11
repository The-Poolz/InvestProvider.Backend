namespace InvestProvider.Backend.Services.Handlers.MyUpcomingAllocation.Models;

public class MyUpcomingAllocationResponse(string projectId, long poolzBackId, decimal amount)
{
    public string ProjectId { get; } = projectId;
    public long PoolzBackId { get; } = poolzBackId;
    public decimal Amount { get; } = amount;
}