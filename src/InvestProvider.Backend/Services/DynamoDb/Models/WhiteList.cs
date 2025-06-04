using Nethereum.Util;
using Net.Web3.EthereumWallet;
using Amazon.DynamoDBv2.DataModel;

namespace InvestProvider.Backend.Services.DynamoDb.Models;

[DynamoDBTable("InvestProvider.WhiteList")]
public class WhiteList(string projectId, DateTime start, EthereumAddress userAddress, decimal amount)
{
    public WhiteList() : this(string.Empty, DateTime.UnixEpoch, EthereumAddress.ZeroAddress, decimal.Zero) { }

    [DynamoDBHashKey("HashId")]
    public string HashId { get; set; } = $"{projectId}-{start.ToUnixTimestamp()}";

    [DynamoDBRangeKey("UserAddress")]
    public string UserAddress { get; set; } = userAddress;

    [DynamoDBProperty("Amount")]
    public decimal Amount { get; set; } = amount;
}