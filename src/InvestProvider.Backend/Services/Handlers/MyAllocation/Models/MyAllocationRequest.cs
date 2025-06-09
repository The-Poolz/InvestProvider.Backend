using MediatR;
using Newtonsoft.Json;
using Net.Web3.EthereumWallet;

namespace InvestProvider.Backend.Services.Handlers.MyAllocation.Models;

public class MyAllocationRequest : IRequest<MyAllocationResponse>
{
    [JsonRequired]
    public string ProjectId { get; set; } = null!;

    [JsonRequired]
    public EthereumAddress UserAddress { get; set; } = null!;
}