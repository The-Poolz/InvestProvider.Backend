using MediatR;
using Nethereum.Util;
using NethereumGenerators.Interfaces;
using InvestProvider.Backend.Services.Web3.Eip712;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.Web3.Eip712.Models;
using InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

namespace InvestProvider.Backend.Services.Handlers.GenerateSignature;

public class GenerateSignatureHandler(
    IChainProvider<ContractType> chainProvider,
    ISignatureGenerator signatureGenerator
)
    : IRequestHandler<GenerateSignatureRequest, GenerateSignatureResponse>
{
    public Task<GenerateSignatureResponse> Handle(GenerateSignatureRequest request, CancellationToken cancellationToken)
    {
        var signature = signatureGenerator.GenerateSignature(
            new Eip712Domain(
                chainId: request.StrapiProjectInfo.ChainId,
                verifyingContract: chainProvider.ContractAddress(request.StrapiProjectInfo.ChainId, ContractType.InvestedProvider)
            ),
            new InvestMessage(
                poolId: request.DynamoDbProjectsInfo.PoolzBackId,
                userAddress: request.UserAddress,
                amount: UnitConversion.Convert.ToWei(request.Amount, request.TokenDecimals),
                validUntil: request.StrapiProjectInfo.CurrentPhase!.Finish!.Value,
                nonce: request.UserInvestments.Length
            )
        );

        return Task.FromResult(new GenerateSignatureResponse(signature,
            request.StrapiProjectInfo.CurrentPhase.Finish!.Value,
            request.DynamoDbProjectsInfo.PoolzBackId
        ));
    }
}