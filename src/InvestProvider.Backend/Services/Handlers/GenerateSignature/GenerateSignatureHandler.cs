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
                chainId: request.PhaseContext.StrapiProjectInfo.ChainId,
                verifyingContract: chainProvider.ContractAddress(request.PhaseContext.StrapiProjectInfo.ChainId, ContractType.InvestedProvider)
            ),
            new InvestMessage(
                poolId: request.PhaseContext.DynamoDbProjectsInfo.PoolzBackId,
                userAddress: request.UserAddress,
                amount: UnitConversion.Convert.ToWei(request.PhaseContext.Amount, request.PhaseContext.TokenDecimals),
                validUntil: request.PhaseContext.StrapiProjectInfo.CurrentPhase!.Finish!.Value,
                nonce: request.PhaseContext.UserInvestments.Length
            )
        );

        return Task.FromResult(new GenerateSignatureResponse(signature,
            request.PhaseContext.StrapiProjectInfo.CurrentPhase.Finish!.Value,
            request.PhaseContext.DynamoDbProjectsInfo.PoolzBackId
        ));
    }
}
