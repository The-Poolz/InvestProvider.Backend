using FluentValidation;
using Net.Utils.ErrorHandler.Extensions;
using Net.Cache.DynamoDb.ERC20;
using InvestProvider.Backend.Services.Web3;
using InvestProvider.Backend.Services.Web3.Contracts;
using poolz.finance.csharp.contracts.LockDealNFT;
using poolz.finance.csharp.contracts.InvestProvider;
using InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

namespace InvestProvider.Backend.Services.Handlers.GenerateSignature;

public partial class GenerateSignatureRequestValidator : BasePhaseValidator<GenerateSignatureRequest>
{
    private readonly IRpcProvider _rpcProvider;
    private readonly ERC20CacheProvider _erc20Cache;
    private readonly ILockDealNFTService<ContractType> _lockDealNFT;
    private readonly IInvestProviderService<ContractType> _investProvider;

    public GenerateSignatureRequestValidator(
        IRpcProvider rpcProvider,
        ERC20CacheProvider erc20Cache,
        ILockDealNFTService<ContractType> lockDealNFT,
        IInvestProviderService<ContractType> investProvider
    )
    {
        _rpcProvider = rpcProvider;
        _erc20Cache = erc20Cache;
        _lockDealNFT = lockDealNFT;
        _investProvider = investProvider;

        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.ProjectId).NotNull().NotEmpty();
        RuleFor(x => x.WeiAmount).NotNull().NotEmpty();

        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(HasCurrentPhase)
            .WithError(Error.NOT_FOUND_ACTIVE_PHASE, x => new { x.ProjectId })
            .Must(HasProjectsInformation)
            .WithError(Error.POOLZ_BACK_ID_NOT_FOUND, x => new { x.ProjectId })
            .MustAsync(MustMoreThanAllowedMinimumAsync)
            .WithError(Error.INVEST_AMOUNT_IS_LESS_THAN_ALLOWED, x => new
            {
                UserAmount = x.Context.Amount,
                MinInvestAmount = _minInvestAmount
            })
            .CustomAsync(SetUserInvestmentsAsync);

        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(x => x.Context.Amount <= x.Context.StrapiProjectInfo!.CurrentPhase!.MaxInvest)
            .WithError(Error.AMOUNT_EXCEED_MAX_INVEST, x => new { x.Context.StrapiProjectInfo!.CurrentPhase!.MaxInvest })
            .Must(x => x.Context.InvestedAmount == 0)
            .WithError(Error.ALREADY_INVESTED)
            .When(x => x.Context.StrapiProjectInfo!.CurrentPhase!.MaxInvest != 0);

        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(HasWhiteList)
            .WithError(Error.NOT_IN_WHITE_LIST, x => new { x.ProjectId, PhaseId = x.Context.StrapiProjectInfo!.CurrentPhase!.Id, UserAddress = x.UserAddress.Address })
            .Must(x => x.Context.Amount + x.Context.InvestedAmount <= x.Context.WhiteList!.Amount)
            .WithError(Error.AMOUNT_EXCEED_MAX_WHITE_LIST_AMOUNT, x => new
            {
                UserAmount = x.Context.Amount,
                MaxInvestAmount = x.Context.WhiteList!.Amount,
                InvestSum = x.Context.InvestedAmount
            })
            .When(x => x.Context.StrapiProjectInfo!.CurrentPhase!.MaxInvest == 0);
    }
}
