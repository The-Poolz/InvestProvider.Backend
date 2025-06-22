using Poolz.Finance.CSharp.Strapi;
using ProjectsInformation = InvestProvider.Backend.Services.DynamoDb.Models.ProjectsInformation;
using InvestProvider.Backend.Services.Strapi.Models;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Web3.Contracts.Models;

namespace InvestProvider.Backend.Services.Validators;

public class PhaseValidationContext
{
    public ProjectInfo StrapiProjectInfo { get; set; } = null!;
    public ProjectsInformation DynamoDbProjectsInfo { get; set; } = null!;
    public ComponentPhaseStartEndAmount Phase { get; set; } = null!;
    public WhiteList WhiteList { get; set; } = null!;
    public UserInvestments[] UserInvestments { get; set; } = Array.Empty<UserInvestments>();
    public decimal InvestedAmount { get; set; }
    public decimal Amount { get; set; }
    public byte TokenDecimals { get; set; }
}
