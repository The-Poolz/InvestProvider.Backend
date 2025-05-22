using MediatR;
using Net.Cache;
using Nethereum.Util;
using System.Numerics;
using Net.Cache.DynamoDb.ERC20;
using Poolz.Finance.CSharp.Strapi;
using Net.Cache.DynamoDb.ERC20.Models;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Web3;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Web3.Eip712;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Web3.Eip712.Models;
using InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

namespace InvestProvider.Backend.Services.Handlers.GenerateSignature;

public class GenerateSignatureHandler(
    IStrapiClient strapi,
    IInvestProviderContract investProvider,
    ILockDealNFTContract lockDealNFT,
    ERC20CacheProvider erc20Cache,
    IChainProvider chainProvider,
    CacheProvider<string, UserData> userProvider,
    ISignatureGenerator signatureGenerator
)
    : IRequestHandler<GenerateSignatureRequest, GenerateSignatureResponse>
{
    public const byte MinInvestAmount = 1;

    public Task<GenerateSignatureResponse> Handle(GenerateSignatureRequest request, CancellationToken cancellationToken)
    {
        var projectInfo = strapi.ReceiveProjectInfo(request.ProjectId);
        if (projectInfo.CurrentPhase == null)
        {
            throw Error.NOT_FOUND_ACTIVE_PHASE.ToException(new
            {
                request.ProjectId
            });
        }

        var tokenAddress = lockDealNFT.TokenOf(projectInfo.ChainId, projectInfo.PoolId);
        var decimals = erc20Cache.GetOrAdd(new GetCacheRequest(
            projectInfo.ChainId,
            tokenAddress,
            chainProvider.RpcUrl(projectInfo.ChainId)
        )).Decimals;

        var amount = UnitConversion.Convert.FromWei(BigInteger.Parse(request.WeiAmount), decimals);
        if (amount < MinInvestAmount)
        {
            throw Error.INVEST_AMOUNT_IS_LESS_THAN_ALLOWED.ToException(new
            {
                UserAmount = amount,
                MinInvestAmount
            });
        }

        if (userProvider.TryGet(projectInfo.CurrentPhase.Id, out var userData))
        {
            throw Error.USER_NOT_FOUND.ToException();
        }
        var userInvestments = investProvider.GetUserInvests(projectInfo.ChainId, projectInfo.PoolId, request.UserAddress).ToArray();
        var investAmounts = userInvestments.Where(x => x.BlockCreation >= projectInfo.CurrentPhase.Start && x.BlockCreation < projectInfo.CurrentPhase.Finish).Sum(x => UnitConversion.Convert.FromWei(x.Amount, decimals));
        if (projectInfo.CurrentPhase.MaxInvest == 0)
        {
            ValidateWhiteList(userData, amount, investAmounts);
        }
        else
        {
            ValidateFCFS(projectInfo.CurrentPhase, amount, investAmounts);
        }

        var signature = signatureGenerator.GenerateSignature(
            new Eip712Domain(projectInfo.ChainId, chainProvider.InvestedProviderContract(projectInfo.ChainId)),
            new InvestMessage(projectInfo.PoolId, request.UserAddress, UnitConversion.Convert.ToWei(amount, decimals), projectInfo.CurrentPhase.Finish!.Value, userInvestments.Length)
        );

        return Task.FromResult(new GenerateSignatureResponse(signature, projectInfo.CurrentPhase.Finish!.Value));
    }

    private static void ValidateFCFS(ComponentPhaseStartEndAmount phase, decimal amount, decimal investSum)
    {
        if (phase.MaxInvest > amount)
        {
            throw Error.AMOUNT_EXCEED_MAX_INVEST.ToException(new
            {
                phase.MaxInvest
            });
        }
        if (investSum > 0)
        {
            throw Error.ALREADY_INVESTED.ToException();
        }
    }

    private static void ValidateWhiteList(UserData userData, decimal amount, decimal investSum)
    {
        if (userData == null)
        {
            throw Error.NOT_IN_WHITE_LIST.ToException();
        }
        if (userData.Amount < amount + investSum)
        {
            throw Error.AMOUNT_EXCEED_MAX_WHITE_LIST_AMOUNT.ToException(new
            {
                UserAmount = amount,
                MaxInvestAmount = userData.Amount,
                InvestSum = investSum
            });
        }
    }
}