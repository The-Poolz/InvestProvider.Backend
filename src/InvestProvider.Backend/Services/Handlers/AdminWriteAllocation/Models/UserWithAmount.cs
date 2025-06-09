using Newtonsoft.Json;
using Net.Web3.EthereumWallet;
using Net.Web3.EthereumWallet.Json.Converters;

namespace InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;

[method: JsonConstructor]
public class UserWithAmount(EthereumAddress userAddress, decimal amount)
{
    [JsonRequired]
    [JsonConverter(typeof(EthereumAddressConverter))]
    public EthereumAddress UserAddress { get; } = userAddress;

    [JsonRequired]
    public decimal Amount { get; } = amount;
}