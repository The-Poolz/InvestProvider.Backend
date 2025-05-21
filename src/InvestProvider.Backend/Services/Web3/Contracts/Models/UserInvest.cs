using System.Numerics;

namespace InvestProvider.Backend.Services.Web3.Contracts.Models;

public class UserInvest(UserInvestRpcOutput userInvest)
{
    public DateTime BlockCreation { get; } = DateTimeOffset.FromUnixTimeSeconds((long)userInvest.BlockTimestamp).UtcDateTime;
    public BigInteger Amount { get; } = userInvest.Amount;
}