using System.Numerics;
using poolz.finance.csharp.contracts.InvestProvider.ContractDefinition;

namespace InvestProvider.Backend.Services.Web3.Contracts.Models;

public class UserInvestments(UserInvest userInvest)
{
    public DateTime BlockCreation { get; } = DateTimeOffset.FromUnixTimeSeconds((long)userInvest.BlockTimestamp).UtcDateTime;
    public BigInteger Amount { get; } = userInvest.Amount;
}