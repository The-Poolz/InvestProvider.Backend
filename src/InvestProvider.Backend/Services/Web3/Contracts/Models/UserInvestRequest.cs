using System.Numerics;
using Nethereum.Contracts;
using Net.Web3.EthereumWallet;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace InvestProvider.Backend.Services.Web3.Contracts.Models;

[Function("getUserInvests", typeof(UserInvestResponse))]
public class UserInvestRequest(BigInteger poolId, EthereumAddress user) : FunctionMessage
{
    public UserInvestRequest() : this(BigInteger.Zero, EthereumAddress.ZeroAddress) { }

    [Parameter("uint256", "poolId", order: 1)]
    public BigInteger PoolId { get; set; } = poolId;

    [Parameter("address", "user", order: 2)]
    public string User { get; set; } = user;
}