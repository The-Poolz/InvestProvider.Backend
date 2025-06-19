using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Web3.Contracts.Models;

namespace InvestProvider.Backend.Services.Validators.Models;

public interface IWhiteListSignature : IHasUserAddress, IValidatedInvestAmount
{
    public UserInvestments[] UserInvestments { get; set; }
    public decimal InvestedAmount { get; set; }
    public WhiteList WhiteList { get; set; }
}