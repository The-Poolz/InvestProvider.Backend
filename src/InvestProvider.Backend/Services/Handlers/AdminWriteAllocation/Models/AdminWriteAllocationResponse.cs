namespace InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;

public class AdminWriteAllocationResponse(int saved)
{
    public int Saved { get; } = saved;
}