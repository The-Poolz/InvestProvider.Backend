using System.Numerics;
using FluentValidation;
using Net.Cache.DynamoDb.ERC20.Models;
using EnvironmentManager.Extensions;
using Nethereum.Util;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.Web3.Contracts.Models;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

namespace InvestProvider.Backend.Services.Handlers.GenerateSignature;

public partial class GenerateSignatureRequestValidator
{
    private readonly byte _minInvestAmount = Env.MIN_INVEST_AMOUNT.GetOrDefault<byte>(1);

    private bool NotNullCurrentPhase(GenerateSignatureRequest model)
    {
        model.StrapiProjectInfo = _strapi.ReceiveProjectInfo(model.ProjectId, filterPhases: model.FilterPhases);
        return model.StrapiProjectInfo.CurrentPhase != null;
    }

    private async Task<bool> NotNullProjectsInformationAsync(GenerateSignatureRequest model, CancellationToken token)
    {
        model.DynamoDbProjectsInfo = await _dynamoDb.LoadAsync<ProjectsInformation>(model.ProjectId, token);
        return model.DynamoDbProjectsInfo != null;
    }

    private async Task<bool> MustMoreThanAllowedMinimumAsync(GenerateSignatureRequest model, CancellationToken _)
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

    private async Task<bool> NotNullWhiteListAsync(GenerateSignatureRequest model, CancellationToken token)
    {
        model.WhiteList = await _dynamoDb.LoadAsync<WhiteList>(
            hashKey: WhiteList.CalculateHashId(model.ProjectId, model.StrapiProjectInfo.CurrentPhase!.Start!.Value),
            rangeKey: model.UserAddress.Address,
            token
        );
        return model.WhiteList != null;
    }
}
