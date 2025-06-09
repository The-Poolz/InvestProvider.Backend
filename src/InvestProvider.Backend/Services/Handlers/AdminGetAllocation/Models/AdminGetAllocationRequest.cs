using MediatR;
using Newtonsoft.Json;

namespace InvestProvider.Backend.Services.Handlers.AdminGetAllocation.Models;

[method: JsonConstructor]
public class AdminGetAllocationRequest(string projectId) : IRequest<AdminGetAllocationResponse>
{
    [JsonRequired]
    public string ProjectId { get; } = projectId;
}