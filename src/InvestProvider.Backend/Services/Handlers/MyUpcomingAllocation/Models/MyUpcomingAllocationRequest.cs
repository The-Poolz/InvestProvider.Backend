using MediatR;
using Newtonsoft.Json;
using Net.Web3.EthereumWallet;

namespace InvestProvider.Backend.Services.Handlers.MyUpcomingAllocation.Models;

[method: JsonConstructor]
public class MyUpcomingAllocationRequest(string[] projectIDs, EthereumAddress userAddress) : IRequest<ICollection<MyUpcomingAllocationResponse>>
{
    [JsonRequired]
    public string[] ProjectIDs { get; } = projectIDs;

    [JsonRequired]
    public EthereumAddress UserAddress { get; } = userAddress;
}