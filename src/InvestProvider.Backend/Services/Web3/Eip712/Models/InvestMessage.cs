using Nethereum.Util;
using System.Numerics;
using Net.Web3.EthereumWallet;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace InvestProvider.Backend.Services.Web3.Eip712.Models;

[Struct(name: "InvestMessage")]
public class InvestMessage(long poolId, EthereumAddress userAddress, BigInteger amount, DateTime validUntil, BigInteger nonce)
{
    [Parameter(type: "uint256", name: "poolId", order: 1)]
    public BigInteger PoolId { get; } = poolId;

    [Parameter(type: "address", name: "user", order: 2)]
    public string UserAddress { get; } = userAddress;

    [Parameter(type: "uint256", name: "amount", order: 3)]
    public BigInteger Amount { get; } = amount;

    [Parameter(type: "uint256", name: "validUntil", order: 4)]
    public BigInteger ValidUntil { get; } = validUntil.ToUnixTimestamp();

    [Parameter(type: "uint256", name: "nonce", order: 5)]
    public BigInteger Nonce { get; } = nonce;
}