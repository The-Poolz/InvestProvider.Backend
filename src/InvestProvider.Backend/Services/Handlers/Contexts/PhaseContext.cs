using Poolz.Finance.CSharp.Strapi;
using InvestProvider.Backend.Services.Strapi.Models;
using ProjectsInformation = InvestProvider.Backend.Services.DynamoDb.Models.ProjectsInformation;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Web3.Contracts.Models;
using Net.Web3.EthereumWallet;

namespace InvestProvider.Backend.Services.Handlers.Contexts;

public class PhaseContext
{
    public required string ProjectId { get; set; }
    public bool FilterPhases { get; set; } = true;

    public string? PhaseId { get; set; }
    public EthereumAddress? UserAddress { get; set; }
    public string? WeiAmount { get; set; }

    public ProjectInfo? StrapiProjectInfo { get; set; }
    public ProjectsInformation? DynamoDbProjectsInfo { get; set; }
    public ComponentPhaseStartEndAmount? Phase { get; set; }
    public WhiteList? WhiteList { get; set; }
    public UserInvestments[]? UserInvestments { get; set; }
    public byte TokenDecimals { get; set; }
    public decimal Amount { get; set; }
    public decimal InvestedAmount { get; set; }
    public long ChainId => StrapiProjectInfo?.ChainId ?? 0;
}

public interface IPhaseRequest
{
    string ProjectId { get; }
    bool FilterPhases { get; }
    string? PhaseId { get; }
    EthereumAddress? UserAddress { get; }
    string? WeiAmount { get; }
    long? ChainId { get; }
    PhaseContext Context { get; set; }
}
