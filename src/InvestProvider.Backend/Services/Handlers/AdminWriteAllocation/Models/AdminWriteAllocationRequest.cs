using MediatR;
using Newtonsoft.Json;

namespace InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;

public class AdminWriteAllocationRequest : IRequest<AdminWriteAllocationResponse>
{
    [JsonRequired]
    public string ProjectId { get; set; } = null!;

    [JsonRequired]
    public string PhaseId { get; set; } = null!;

    [JsonRequired]
    public ICollection<UserWithAmount> Users { get; set; } = null!;
}