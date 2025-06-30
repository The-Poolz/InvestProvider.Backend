using Poolz.Finance.CSharp.Strapi;
using InvestProvider.Backend.Services.Strapi.Models;
using ProjectsInformation = InvestProvider.Backend.Services.DynamoDb.Models.ProjectsInformation;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Web3.Contracts.Models;
using Net.Web3.EthereumWallet;

namespace InvestProvider.Backend.Services.Handlers.Contexts;

public interface IProjectContext
{
    string ProjectId { get; }
    string PhaseId { get; }
    long ChainId { get; }
}

public interface IExistActivePhase : IProjectContext
{
    bool FilterPhases { get; }
    ProjectInfo StrapiProjectInfo { get; set; }
}

public interface IExistPhase : IExistActivePhase
{
    ComponentPhaseStartEndAmount Phase { get; set; }
}

public interface IValidatedDynamoDbProjectInfo : IProjectContext
{
    ProjectsInformation DynamoDbProjectsInfo { get; set; }
}

public interface IValidatedInvestAmount : IExistActivePhase, IValidatedDynamoDbProjectInfo
{
    decimal Amount { get; set; }
    byte TokenDecimals { get; set; }
    string WeiAmount { get; }
}

public interface IHasUserInvestments : IValidatedInvestAmount
{
    UserInvestments[] UserInvestments { get; set; }
    decimal InvestedAmount { get; set; }
    EthereumAddress UserAddress { get; }
}

public interface IInvestPool : IProjectContext
{
    long PoolzBackId { get; }
}

public interface IWhiteListPhase
{
    ComponentPhaseStartEndAmount Phase { get; }
}

public interface IWhiteListUser : IExistActivePhase
{
    WhiteList WhiteList { get; set; }
    EthereumAddress UserAddress { get; }
}

public interface IWhiteListSignature : IHasUserInvestments
{
    WhiteList WhiteList { get; set; }
}
