using System.Numerics;
using FluentValidation;
using Net.Cache.DynamoDb.ERC20.Models;
using EnvironmentManager.Extensions;
using Nethereum.Util;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.Web3.Contracts.Models;
using InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

namespace InvestProvider.Backend.Services.Handlers.GenerateSignature;

public partial class GenerateSignatureRequestValidator
{
    private readonly decimal _minInvestAmount = Env.MIN_INVEST_AMOUNT.GetOrDefault<decimal>(1);

    private async Task<bool> MustMoreThanAllowedMinimumAsync(GenerateSignatureRequest model, CancellationToken _)
    {
        var tokenAddress = await _lockDealNFT.TokenOfQueryAsync(
            model.Context.StrapiProjectInfo!.ChainId,
            ContractType.LockDealNFT,
            model.Context.DynamoDbProjectsInfo!.PoolzBackId
        );

        model.Context.TokenDecimals = _erc20Cache.GetOrAdd(new GetCacheRequest(
            model.Context.StrapiProjectInfo!.ChainId,
            tokenAddress,
            _rpcProvider.RpcUrl(model.Context.StrapiProjectInfo!.ChainId)
        )).Decimals;

        model.Context.Amount = UnitConversion.Convert.FromWei(BigInteger.Parse(model.WeiAmount), model.Context.TokenDecimals);

        return model.Context.Amount >= _minInvestAmount;
    }

    private async Task SetUserInvestmentsAsync(GenerateSignatureRequest model, ValidationContext<GenerateSignatureRequest> context, CancellationToken _)
    {
        var response = await _investProvider.GetUserInvestsQueryAsync(
            model.Context.StrapiProjectInfo!.ChainId,
            ContractType.InvestedProvider,
            model.Context.DynamoDbProjectsInfo!.PoolzBackId,
            model.Context.UserAddress!
        );

        model.Context.UserInvestments = response.ReturnValue1
            .Select(ui => new UserInvestments(ui))
            .ToArray();

        model.Context.InvestedAmount = model.Context.UserInvestments
            .Where(ui => ui.BlockCreation >= model.Context.StrapiProjectInfo!.CurrentPhase!.Start && ui.BlockCreation < model.Context.StrapiProjectInfo.CurrentPhase.Finish)
            .Sum(ui => UnitConversion.Convert.FromWei(ui.Amount, model.Context.TokenDecimals));
    }

}
