namespace InvestProvider.Backend.Services.Handlers.MyAllocation.Models;

public class MyAllocationResponse(decimal amount, DateTime startTime, DateTime endTime, long poolzBackId)
{
    public decimal Amount { get; } = amount;
    public DateTime StartTime { get; } = startTime;
    public DateTime EndTime { get; } = endTime;
    public long PoolzBackId { get; } = poolzBackId;
}