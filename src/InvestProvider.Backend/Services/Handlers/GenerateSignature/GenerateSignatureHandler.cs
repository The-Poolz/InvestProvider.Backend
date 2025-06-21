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
using InvestProvider.Backend.Services.Strapi;
using poolz.finance.csharp.contracts.LockDealNFT;
using InvestProvider.Backend.Services.Web3.Eip712;
using poolz.finance.csharp.contracts.InvestProvider;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Web3.Eip712.Models;
using InvestProvider.Backend.Services.Web3.Contracts.Models;
using InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;
using ProjectsInformation = InvestProvider.Backend.Services.DynamoDb.Models.ProjectsInformation;

namespace InvestProvider.Backend.Services.Handlers.GenerateSignature;

public class GenerateSignatureHandler(
    IStrapiClient strapi,
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
        var projectInfo = strapi.ReceiveProjectInfo(request.ProjectId, filterPhases: true);
        if (projectInfo.CurrentPhase == null)
        {
            throw Error.NOT_FOUND_ACTIVE_PHASE.ToException(new
            {
                request.ProjectId
            });
        }

        var dynamoProjectInfo = await dynamoDb.LoadAsync<ProjectsInformation>(request.ProjectId, cancellationToken);
        if (dynamoProjectInfo == null)
        {
            throw Error.POOLZ_BACK_ID_NOT_FOUND.ToException(new
            {
                request.ProjectId
            });
        }

        var tokenAddress = await lockDealNFT.TokenOfQueryAsync(projectInfo.ChainId, ContractType.LockDealNFT, dynamoProjectInfo.PoolzBackId);
        var decimals = erc20Cache.GetOrAdd(new GetCacheRequest(
            projectInfo.ChainId,
            tokenAddress,
            rpcProvider.RpcUrl(projectInfo.ChainId)
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

        var userInvestmentsResponse = await investProvider.GetUserInvestsQueryAsync(projectInfo.ChainId, ContractType.InvestedProvider, dynamoProjectInfo.PoolzBackId, request.UserAddress);
        var userInvestments = userInvestmentsResponse.ReturnValue1.Select(x => new UserInvestments(x)).ToArray();

        var investAmounts = userInvestments
            .Where(x => x.BlockCreation >= projectInfo.CurrentPhase.Start && x.BlockCreation < projectInfo.CurrentPhase.Finish)
            .Sum(x => UnitConversion.Convert.FromWei(x.Amount, decimals));

        if (projectInfo.CurrentPhase.MaxInvest == 0)
        {
            var whiteList = await dynamoDb.LoadAsync<WhiteList>(WhiteList.CalculateHashId(request.ProjectId, projectInfo.CurrentPhase.Start!.Value), request.UserAddress.Address, cancellationToken);
            if (whiteList == null) throw Error.USER_NOT_FOUND.ToException();
            ValidateWhiteList(whiteList, amount, investAmounts);
        }
        else
        {
            ValidateFCFS(projectInfo.CurrentPhase, amount, investAmounts);
        }

        var signature = signatureGenerator.GenerateSignature(
            new Eip712Domain(projectInfo.ChainId, chainProvider.ContractAddress(projectInfo.ChainId, ContractType.InvestedProvider)),
            new InvestMessage(dynamoProjectInfo.PoolzBackId, request.UserAddress, UnitConversion.Convert.ToWei(amount, decimals), projectInfo.CurrentPhase.Finish!.Value, userInvestments.Length)
        );

        return new GenerateSignatureResponse(signature, projectInfo.CurrentPhase.Finish!.Value, dynamoProjectInfo.PoolzBackId);
    }

    private static void ValidateFCFS(ComponentPhaseStartEndAmount phase, decimal amount, decimal investSum)
    {
        // Ensure the user doesn't try to invest more than allowed in FCFS mode
        // phase.MaxInvest represents the maximum allowed amount per user
        // so we should validate the requested amount does not exceed this limit
        if (amount > phase.MaxInvest)
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