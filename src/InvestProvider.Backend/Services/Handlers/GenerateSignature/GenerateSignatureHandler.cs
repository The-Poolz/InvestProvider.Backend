using MediatR;
using Nethereum.Util;
using Amazon.DynamoDBv2.DataModel;
using Poolz.Finance.CSharp.Strapi;
using NethereumGenerators.Interfaces;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Web3.Eip712;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Web3.Eip712.Models;
using InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

namespace InvestProvider.Backend.Services.Handlers.GenerateSignature;

public class GenerateSignatureHandler(
    IChainProvider<ContractType> chainProvider,
    IDynamoDBContext dynamoDb,
    ISignatureGenerator signatureGenerator
)
    : IRequestHandler<GenerateSignatureRequest, GenerateSignatureResponse>
{
    public async Task<GenerateSignatureResponse> Handle(GenerateSignatureRequest request, CancellationToken cancellationToken)
    {
        if (request.StrapiProjectInfo.CurrentPhase!.MaxInvest == 0)
        {
            var whiteList = await dynamoDb.LoadAsync<WhiteList>(
                hashKey: WhiteList.CalculateHashId(request.ProjectId, request.StrapiProjectInfo.CurrentPhase.Start!.Value), 
                rangeKey: request.UserAddress.Address,
                cancellationToken
            );
            if (whiteList == null) throw Error.USER_NOT_FOUND.ToException();
            ValidateWhiteList(whiteList, request.Amount, investAmounts);
        }
        else
        {
            ValidateFCFS(request.StrapiProjectInfo.CurrentPhase, request.Amount, investAmounts);
        }

        var signature = signatureGenerator.GenerateSignature(
            new Eip712Domain(request.StrapiProjectInfo.ChainId, chainProvider.ContractAddress(request.StrapiProjectInfo.ChainId, ContractType.InvestedProvider)),
            new InvestMessage(request.DynamoDbProjectsInfo.PoolzBackId, request.UserAddress, UnitConversion.Convert.ToWei(request.Amount, request.TokenDecimals), request.StrapiProjectInfo.CurrentPhase.Finish!.Value, userInvestments.Length)
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