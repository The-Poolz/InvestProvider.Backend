using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace InvestProvider.Backend.Services.Web3.Contracts.Models;

[FunctionOutput]
public class UserInvest
{
    [Parameter("uint256", "blockTimestamp", order: 1)]
    public BigInteger BlockTimestamp { get; set; }

    [Parameter("uint256", "amount", order: 2)]
    public BigInteger Amount { get; set; }
}