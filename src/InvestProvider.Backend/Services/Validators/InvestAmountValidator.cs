using Nethereum.Util;
using System.Numerics;
using FluentValidation;
using Net.Cache.DynamoDb.ERC20;
using EnvironmentManager.Extensions;
using Net.Cache.DynamoDb.ERC20.Models;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Web3;
using poolz.finance.csharp.contracts.LockDealNFT;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.Validators.Models;

namespace InvestProvider.Backend.Services.Validators;

public class InvestAmountValidator : AbstractValidator<IValidatedInvestAmount>
{
    private readonly IRpcProvider _rpcProvider;
    private readonly ERC20CacheProvider _erc20Cache;
    private readonly ILockDealNFTService<ContractType> _lockDealNFT;
    public readonly byte MinInvestAmount = Env.MIN_INVEST_AMOUNT.GetOrDefault<byte>(1);

    public InvestAmountValidator(
        IRpcProvider rpcProvider,
        ERC20CacheProvider erc20Cache,
        ILockDealNFTService<ContractType> lockDealNFT
    )
    {
        _rpcProvider = rpcProvider;
        _erc20Cache = erc20Cache;
        _lockDealNFT = lockDealNFT;

        RuleFor(x => x)
            .MustAsync((x, _) => MustMoreThanAllowedMinimumAsync(x))
            .WithError(Error.INVEST_AMOUNT_IS_LESS_THAN_ALLOWED, x => new
            {
                UserAmount = x.Amount,
                MinInvestAmount = UnitConversion.Convert.FromWei(MinInvestAmount, x.TokenDecimals)
            });
    }

    private async Task<bool> MustMoreThanAllowedMinimumAsync(IValidatedInvestAmount model)
    {
        var tokenAddress = await _lockDealNFT.TokenOfQueryAsync(
            model.StrapiProjectInfo.ChainId,
            ContractType.LockDealNFT,
            model.DynamoDbProjectsInfo.PoolzBackId
        );

        model.TokenDecimals = _erc20Cache.GetOrAdd(new GetCacheRequest(
            model.StrapiProjectInfo.ChainId,
            tokenAddress,
            _rpcProvider.RpcUrl(model.StrapiProjectInfo.ChainId)
        )).Decimals;

        model.Amount = UnitConversion.Convert.FromWei(BigInteger.Parse(model.WeiAmount), model.TokenDecimals);

        return model.Amount >= MinInvestAmount;
    }
}