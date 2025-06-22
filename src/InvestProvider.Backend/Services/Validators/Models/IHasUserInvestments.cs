using InvestProvider.Backend.Services.Web3.Contracts.Models;

namespace InvestProvider.Backend.Services.Validators.Models;

public interface IHasUserInvestments : IHasUserAddress, IValidatedInvestAmount
{
    public UserInvestments[] UserInvestments { get; set; }
    public decimal InvestedAmount { get; set; }
}
