using FluentValidation;
using Net.Utils.ErrorHandler.Extensions;
using Amazon.DynamoDBv2.DataModel;
using Net.Cache.DynamoDb.ERC20;
using Net.Cache.DynamoDb.ERC20.Models;
using EnvironmentManager.Extensions;
using Nethereum.Util;
using System.Numerics;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Web3;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.Web3.Contracts.Models;
using InvestProvider.Backend.Services.DynamoDb.Models;
using poolz.finance.csharp.contracts.LockDealNFT;
using poolz.finance.csharp.contracts.InvestProvider;
using InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

namespace InvestProvider.Backend.Services.Handlers.GenerateSignature;

public partial class GenerateSignatureRequestValidator : AbstractValidator<GenerateSignatureRequest>
{
    private readonly IStrapiClient _strapi;
    private readonly IDynamoDBContext _dynamoDb;
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
    )
    {
        _strapi = strapi;
        _dynamoDb = dynamoDb;
        _rpcProvider = rpcProvider;
        _erc20Cache = erc20Cache;
        _lockDealNFT = lockDealNFT;
        _investProvider = investProvider;

        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.ProjectId).NotNull().NotEmpty();
        RuleFor(x => x.WeiAmount).NotNull().NotEmpty();

        RuleFor(x => x)
            .Must(NotNullCurrentPhase)
            .WithError(Error.NOT_FOUND_ACTIVE_PHASE, x => new { x.ProjectId });

        RuleFor(x => x)
            .MustAsync(NotNullProjectsInformationAsync)
            .WithError(Error.POOLZ_BACK_ID_NOT_FOUND, x => new { x.ProjectId });

        RuleFor(x => x)
            .MustAsync(MustMoreThanAllowedMinimumAsync)
            .WithError(Error.INVEST_AMOUNT_IS_LESS_THAN_ALLOWED, x => new
            {
                UserAmount = x.Amount,
                MinInvestAmount = UnitConversion.Convert.FromWei(_minInvestAmount, x.TokenDecimals)
            });

        RuleFor(x => x)
            .CustomAsync(SetUserInvestmentsAsync)
            .WhenAsync((x, _) => Task.FromResult(true));

        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(x => x.Amount <= x.StrapiProjectInfo.CurrentPhase!.MaxInvest)
            .WithError(Error.AMOUNT_EXCEED_MAX_INVEST, x => new { x.StrapiProjectInfo.CurrentPhase!.MaxInvest })
            .Must(x => x.InvestedAmount == 0)
            .WithError(Error.ALREADY_INVESTED)
            .When(x => x.StrapiProjectInfo.CurrentPhase!.MaxInvest != 0);

        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .MustAsync(NotNullWhiteListAsync)
            .WithError(Error.NOT_IN_WHITE_LIST, x => new { x.ProjectId, PhaseId = x.StrapiProjectInfo.CurrentPhase!.Id, UserAddress = x.UserAddress.Address })
            .Must(x => x.Amount + x.InvestedAmount <= x.WhiteList.Amount)
            .WithError(Error.AMOUNT_EXCEED_MAX_WHITE_LIST_AMOUNT, x => new
            {
                UserAmount = x.Amount,
                MaxInvestAmount = x.WhiteList.Amount,
                InvestSum = x.InvestedAmount
            })
            .When(x => x.StrapiProjectInfo.CurrentPhase!.MaxInvest == 0);
    }
}
