using Nethereum.Util;
using System.Numerics;
using FluentValidation;
using Net.Cache.DynamoDb.ERC20;
using Net.Cache.DynamoDb.ERC20.Models;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Web3;
using poolz.finance.csharp.contracts.LockDealNFT;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.Validators.Models;

namespace InvestProvider.Backend.Services.Validators;

public class InvestAmountValidator : AbstractValidator<IValidatedInvestAmount>
{
    public const byte MinInvestAmount = 1;

    public InvestAmountValidator(
        IRpcProvider rpcProvider,
        ERC20CacheProvider erc20Cache,
        ILockDealNFTService<ContractType> lockDealNFT
    )
    {
        RuleFor(x => x)
            .MustAsync(async (x, _) =>
            {
                var tokenAddress = await lockDealNFT.TokenOfQueryAsync(
                    x.StrapiProjectInfo.ChainId,
                    ContractType.LockDealNFT,
                    x.DynamoDbProjectsInfo.PoolzBackId
                );

                x.TokenDecimals = erc20Cache.GetOrAdd(new GetCacheRequest(
                    x.StrapiProjectInfo.ChainId,
                    tokenAddress,
                    rpcProvider.RpcUrl(x.StrapiProjectInfo.ChainId)
                )).Decimals;

                x.Amount = UnitConversion.Convert.FromWei(BigInteger.Parse(x.WeiAmount), x.TokenDecimals);

                return x.Amount >= MinInvestAmount;
            })
            .WithError(Error.INVEST_AMOUNT_IS_LESS_THAN_ALLOWED, new
            {
                MinInvestAmount
            });
    }
}