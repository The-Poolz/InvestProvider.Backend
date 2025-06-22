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
    private readonly byte _minInvestAmount = Env.MIN_INVEST_AMOUNT.GetOrDefault<byte>(1);


    private async Task<bool> MustMoreThanAllowedMinimumAsync(GenerateSignatureRequest model, CancellationToken _)
    {
        var tokenAddress = await _lockDealNFT.TokenOfQueryAsync(
            model.PhaseContext.StrapiProjectInfo.ChainId,
            ContractType.LockDealNFT,
            model.PhaseContext.DynamoDbProjectsInfo.PoolzBackId
        );

        model.PhaseContext.TokenDecimals = _erc20Cache.GetOrAdd(new GetCacheRequest(
            model.PhaseContext.StrapiProjectInfo.ChainId,
            tokenAddress,
            _rpcProvider.RpcUrl(model.PhaseContext.StrapiProjectInfo.ChainId)
        )).Decimals;

        model.PhaseContext.Amount = UnitConversion.Convert.FromWei(BigInteger.Parse(model.WeiAmount), model.PhaseContext.TokenDecimals);

        return model.PhaseContext.Amount >= _minInvestAmount;
    }

    private async Task SetUserInvestmentsAsync(GenerateSignatureRequest model, ValidationContext<GenerateSignatureRequest> context, CancellationToken _)
    {
        var response = await _investProvider.GetUserInvestsQueryAsync(
            model.PhaseContext.StrapiProjectInfo.ChainId,
            ContractType.InvestedProvider,
            model.PhaseContext.DynamoDbProjectsInfo.PoolzBackId,
            model.UserAddress
        );

        model.PhaseContext.UserInvestments = response.ReturnValue1
            .Select(ui => new UserInvestments(ui))
            .ToArray();

        model.PhaseContext.InvestedAmount = model.PhaseContext.UserInvestments
            .Where(ui => ui.BlockCreation >= model.PhaseContext.StrapiProjectInfo.CurrentPhase!.Start && ui.BlockCreation < model.PhaseContext.StrapiProjectInfo.CurrentPhase.Finish)
            .Sum(ui => UnitConversion.Convert.FromWei(ui.Amount, model.PhaseContext.TokenDecimals));
    }

}
