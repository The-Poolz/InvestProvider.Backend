using System.Numerics;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace InvestProvider.Backend.Services.Web3.Contracts.Models;

[Function("tokenOf", "address")]
public class TokenOfRequest(BigInteger poolId) : FunctionMessage
{
    public TokenOfRequest() : this(BigInteger.Zero) { }

    [Parameter("uint256", "poolId", order: 1)]
    public BigInteger PoolId { get; set; } = poolId;
}