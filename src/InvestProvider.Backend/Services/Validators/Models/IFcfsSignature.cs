using InvestProvider.Backend.Services.Strapi.Models;

namespace InvestProvider.Backend.Services.Validators.Models;

public interface IFcfsSignature
{
    public decimal InvestedAmount { get; set; }
    public decimal Amount { get; set; }
    public ProjectInfo StrapiProjectInfo { get; set; }
}