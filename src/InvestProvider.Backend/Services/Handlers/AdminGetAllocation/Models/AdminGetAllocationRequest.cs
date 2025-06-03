using MediatR;
using Newtonsoft.Json;

namespace InvestProvider.Backend.Services.Handlers.AdminGetAllocation.Models;

public class AdminGetAllocationRequest : IRequest<AdminGetAllocationResponse>
{
    [JsonRequired]
    public string ProjectId { get; set; } = null!;
}