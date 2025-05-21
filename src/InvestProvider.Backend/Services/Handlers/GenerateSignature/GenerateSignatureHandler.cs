using MediatR;
using Net.Cache;
using Nethereum.Util;
using System.Numerics;
using Net.Cache.DynamoDb.ERC20;
using Net.Cache.DynamoDb.ERC20.Models;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Web3;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Strapi.Models;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;
using InvestProvider.Backend.Services.Web3.Eip712;
using InvestProvider.Backend.Services.Web3.Eip712.Models;

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
        var phase = strapi.ReceiveProjectPhase(request.PhaseId);
        if (phase.StartTime >= DateTime.UtcNow || phase.EndTime < DateTime.UtcNow)
        {
            throw Error.PHASE_INACTIVE.ToException(new
            {
                request.PhaseId,
                phase.StartTime,
                phase.EndTime
            });
        }

        var tokenAddress = lockDealNFT.TokenOf(phase.ChainId, phase.PoolId);
        var decimals = erc20Cache.GetOrAdd(new GetCacheRequest(
            phase.ChainId,
            tokenAddress,
            chainProvider.RpcUrl(phase.ChainId)
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

        if (userProvider.TryGet(request.PhaseId, out var userData))
        {
            throw Error.USER_NOT_FOUND.ToException();
        }
        var userInvestments = investProvider.GetUserInvests(phase.ChainId, phase.PoolId, request.UserAddress).ToArray();
        var investAmounts = userInvestments.Where(x => x.BlockCreation >= phase.StartTime && x.BlockCreation < phase.EndTime).Sum(x => UnitConversion.Convert.FromWei(x.Amount, decimals));
        if (phase.MaxInvest == 0)
        {
            ValidateWhiteList(userData, amount, investAmounts);
        }
        else
        {
            ValidateFCFS(phase, amount, investAmounts);
        }

        var signature = signatureGenerator.GenerateSignature(
            new Eip712Domain(phase.ChainId, chainProvider.InvestedProviderContract(phase.ChainId)),
            new InvestMessage(phase.PoolId, request.UserAddress, UnitConversion.Convert.ToWei(amount, decimals), phase.EndTime, userInvestments.Length)
        );

        return Task.FromResult(new GenerateSignatureResponse()
        {
            Signature = signature,
            ValidUntil = phase.EndTime
        });

        //return $"{poolId} {Address} {currentPhase.End} {Value} {userInvests.Count()}";
    }

    private static void ValidateFCFS(ProjectPhase phase, decimal amount, decimal investSum)
    {
        if (phase.MaxInvest > amount)
        {
            throw new InvalidOperationException($"Value exceed the MaxInvest = {phase.MaxInvest}");
        }
        if (investSum > 0)
        {
            throw new InvalidOperationException("Already Invested");
        }
    }

    private static void ValidateWhiteList(UserData userData, decimal amount, decimal investSum)
    {
        if (userData == null)
        {
            throw new InvalidOperationException("User not in WhiteList");
        }
        if (userData.Amount < amount + investSum)
        {
            throw new InvalidOperationException($"Value exceed the Amount {amount} {investSum} {userData.Amount}");
        }
    }
}