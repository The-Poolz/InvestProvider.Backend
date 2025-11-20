using Nethereum.Util;
using System.Numerics;
using FluentValidation;
using Net.Web3.EthereumWallet;
using EnvironmentManager.Extensions;
using Net.Cache.DynamoDb.ERC20.DynamoDb.Models;
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
            model.StrapiProjectInfo.ChainId,
            ContractType.LockDealNFT,
            model.DynamoDbProjectsInfo.PoolzBackId
        );

        var web3 = _chainProvider.Web3(model.StrapiProjectInfo.ChainId);
        model.TokenDecimals = _erc20Cache.GetOrAddAsync(
            new HashKey(model.StrapiProjectInfo.ChainId, tokenAddress),
            () => Task.FromResult(web3),
            () => Task.FromResult<EthereumAddress>(Env.MULTI_CALL_V3_ADDRESS.GetRequired())
        ).GetAwaiter().GetResult().Decimals;

        model.Amount = UnitConversion.Convert.FromWei(BigInteger.Parse(model.WeiAmount), model.TokenDecimals);

        return model.Amount >= _minInvestAmount;
    }

    private async Task SetUserInvestmentsAsync(GenerateSignatureRequest model, ValidationContext<GenerateSignatureRequest> context, CancellationToken _)
    {
        var response = await _investProvider.GetUserInvestsQueryAsync(
            model.StrapiProjectInfo.ChainId,
            ContractType.InvestedProvider,
            model.DynamoDbProjectsInfo.PoolzBackId,
            model.UserAddress
        );

        model.UserInvestments = response.ReturnValue1
            .Select(ui => new UserInvestments(ui))
            .ToArray();

        model.InvestedAmount = model.UserInvestments
            .Where(ui => ui.BlockCreation >= model.StrapiProjectInfo.CurrentPhase!.Start && ui.BlockCreation < model.StrapiProjectInfo.CurrentPhase.Finish)
            .Sum(ui => UnitConversion.Convert.FromWei(ui.Amount, model.TokenDecimals));
    }

}
