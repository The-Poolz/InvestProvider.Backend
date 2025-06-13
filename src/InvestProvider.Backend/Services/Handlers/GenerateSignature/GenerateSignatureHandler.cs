using MediatR;
using Nethereum.Util;
using System.Numerics;
using Net.Cache.DynamoDb.ERC20;
using Amazon.DynamoDBv2.DataModel;
using Poolz.Finance.CSharp.Strapi;
using NethereumGenerators.Interfaces;
using Net.Cache.DynamoDb.ERC20.Models;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Web3;
using poolz.finance.csharp.contracts.LockDealNFT;
using InvestProvider.Backend.Services.Web3.Eip712;
using poolz.finance.csharp.contracts.InvestProvider;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Web3.Eip712.Models;
using InvestProvider.Backend.Services.Web3.Contracts.Models;
using InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

namespace InvestProvider.Backend.Services.Handlers.GenerateSignature;

public class GenerateSignatureHandler(
    IInvestProviderService<ContractType> investProvider,
    IChainProvider<ContractType> chainProvider,
    ILockDealNFTService<ContractType> lockDealNFT,
    ERC20CacheProvider erc20Cache,
    IRpcProvider rpcProvider,
    IDynamoDBContext dynamoDb,
    ISignatureGenerator signatureGenerator
)
    : IRequestHandler<GenerateSignatureRequest, GenerateSignatureResponse>
{
    public const byte MinInvestAmount = 1;

    public async Task<GenerateSignatureResponse> Handle(GenerateSignatureRequest request, CancellationToken cancellationToken)
    {
        var tokenAddress = await lockDealNFT.TokenOfQueryAsync(
            request.StrapiProjectInfo.ChainId,
            ContractType.LockDealNFT,
            request.DynamoDbProjectsInfo.PoolzBackId
        );
        var decimals = erc20Cache.GetOrAdd(new GetCacheRequest(
            request.StrapiProjectInfo.ChainId,
            tokenAddress,
            rpcProvider.RpcUrl(request.StrapiProjectInfo.ChainId)
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

        var userInvestmentsResponse = await investProvider.GetUserInvestsQueryAsync(
            request.StrapiProjectInfo.ChainId,
            ContractType.InvestedProvider,
            request.DynamoDbProjectsInfo.PoolzBackId,
            request.UserAddress
        );
        var userInvestments = userInvestmentsResponse.ReturnValue1.Select(x => new UserInvestments(x)).ToArray();

        var investAmounts = userInvestments
            .Where(x =>
                x.BlockCreation >= request.StrapiProjectInfo.CurrentPhase!.Start &&
                x.BlockCreation < request.StrapiProjectInfo.CurrentPhase.Finish
            )
            .Sum(x => UnitConversion.Convert.FromWei(x.Amount, decimals));

        if (request.StrapiProjectInfo.CurrentPhase!.MaxInvest == 0)
        {
            var whiteList = await dynamoDb.LoadAsync<WhiteList>(
                hashKey: WhiteList.CalculateHashId(request.ProjectId, request.StrapiProjectInfo.CurrentPhase.Start!.Value), 
                rangeKey: request.UserAddress.Address,
                cancellationToken
            );
            if (whiteList == null) throw Error.USER_NOT_FOUND.ToException();
            ValidateWhiteList(whiteList, amount, investAmounts);
        }
        else
        {
            ValidateFCFS(request.StrapiProjectInfo.CurrentPhase, amount, investAmounts);
        }

        var signature = signatureGenerator.GenerateSignature(
            new Eip712Domain(request.StrapiProjectInfo.ChainId, chainProvider.ContractAddress(request.StrapiProjectInfo.ChainId, ContractType.InvestedProvider)),
            new InvestMessage(request.DynamoDbProjectsInfo.PoolzBackId, request.UserAddress, UnitConversion.Convert.ToWei(amount, decimals), request.StrapiProjectInfo.CurrentPhase.Finish!.Value, userInvestments.Length)
        );

        return new GenerateSignatureResponse(signature, request.StrapiProjectInfo.CurrentPhase.Finish!.Value, request.DynamoDbProjectsInfo.PoolzBackId);
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

    private static void ValidateWhiteList(WhiteList userData, decimal amount, decimal investSum)
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