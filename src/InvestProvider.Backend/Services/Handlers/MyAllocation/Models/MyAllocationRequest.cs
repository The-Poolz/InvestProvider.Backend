using MediatR;
using Newtonsoft.Json;
using Net.Web3.EthereumWallet;

namespace InvestProvider.Backend.Services.Handlers.MyAllocation.Models;

[method: JsonConstructor]
public class MyAllocationRequest(string projectId, EthereumAddress userAddress) : IRequest<MyAllocationResponse>
{
    [JsonRequired]
    public string ProjectId { get; } = projectId;

    [JsonRequired]
    public EthereumAddress UserAddress { get; } = userAddress;
}