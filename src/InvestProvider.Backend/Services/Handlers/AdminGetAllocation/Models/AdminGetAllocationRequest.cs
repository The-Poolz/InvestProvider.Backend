using MediatR;
using Newtonsoft.Json;

namespace InvestProvider.Backend.Services.Handlers.AdminGetAllocation.Models;

[method: JsonConstructor]
public class AdminGetAllocationRequest(string projectId) : IRequest<ICollection<AdminGetAllocationResponse>>
{
    [JsonRequired]
    public string ProjectId { get; } = projectId;
}