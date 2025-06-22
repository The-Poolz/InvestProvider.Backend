using FluentValidation;
using Net.Utils.ErrorHandler.Extensions;
using Amazon.DynamoDBv2.DataModel;
using Net.Cache.DynamoDb.ERC20;
using Nethereum.Util;
using InvestProvider.Backend.Services.Strapi;
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
        IStrapiClient strapi,
        IDynamoDBContext dynamoDb,
        IRpcProvider rpcProvider,
        ERC20CacheProvider erc20Cache,
        ILockDealNFTService<ContractType> lockDealNFT,
        IInvestProviderService<ContractType> investProvider
    ) : base(strapi, dynamoDb)
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
            .Must(NotNullCurrentPhase)
            .WithError(Error.NOT_FOUND_ACTIVE_PHASE, x => new { x.ProjectId })
            .MustAsync(NotNullProjectsInformationAsync)
            .WithError(Error.POOLZ_BACK_ID_NOT_FOUND, x => new { x.ProjectId })
            .MustAsync(MustMoreThanAllowedMinimumAsync)
            .WithError(Error.INVEST_AMOUNT_IS_LESS_THAN_ALLOWED, x => new
            {
                UserAmount = x.PhaseContext.Amount,
                MinInvestAmount = UnitConversion.Convert.FromWei(_minInvestAmount, x.PhaseContext.TokenDecimals)
            })
            .CustomAsync(SetUserInvestmentsAsync);

        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(x => x.PhaseContext.Amount <= x.PhaseContext.StrapiProjectInfo.CurrentPhase!.MaxInvest)
            .WithError(Error.AMOUNT_EXCEED_MAX_INVEST, x => new { x.PhaseContext.StrapiProjectInfo.CurrentPhase!.MaxInvest })
            .Must(x => x.PhaseContext.InvestedAmount == 0)
            .WithError(Error.ALREADY_INVESTED)
            .When(x => x.PhaseContext.StrapiProjectInfo.CurrentPhase!.MaxInvest != 0);

        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .MustAsync(NotNullWhiteListAsync)
            .WithError(Error.NOT_IN_WHITE_LIST, x => new { x.ProjectId, PhaseId = x.PhaseContext.StrapiProjectInfo.CurrentPhase!.Id, UserAddress = x.UserAddress.Address })
            .Must(x => x.PhaseContext.Amount + x.PhaseContext.InvestedAmount <= x.PhaseContext.WhiteList.Amount)
            .WithError(Error.AMOUNT_EXCEED_MAX_WHITE_LIST_AMOUNT, x => new
            {
                UserAmount = x.PhaseContext.Amount,
                MaxInvestAmount = x.PhaseContext.WhiteList.Amount,
                InvestSum = x.PhaseContext.InvestedAmount
            })
            .When(x => x.PhaseContext.StrapiProjectInfo.CurrentPhase!.MaxInvest == 0);
    }
}
