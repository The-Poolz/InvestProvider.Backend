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
    public Task<GenerateSignatureResponse> Handle(GenerateSignatureRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new GenerateSignatureResponse(
            GetSignature(chainProvider, signatureGenerator, request),
            request.Context.StrapiProjectInfo!.CurrentPhase!.Finish!.Value,
            request.Context.DynamoDbProjectsInfo!.PoolzBackId
        ));

    private static string GetSignature(
        IChainProvider<ContractType> chainProvider,
        ISignatureGenerator signatureGenerator,
        GenerateSignatureRequest request) =>
        signatureGenerator.GenerateSignature(
            new Eip712Domain(
                chainId: request.Context.StrapiProjectInfo!.ChainId,
                verifyingContract: chainProvider.ContractAddress(request.Context.StrapiProjectInfo!.ChainId, ContractType.InvestedProvider)
            ),
            new InvestMessage(
                poolId: request.Context.DynamoDbProjectsInfo!.PoolzBackId,
                userAddress: request.Context.UserAddress!,
                amount: UnitConversion.Convert.ToWei(request.Context.Amount, request.Context.TokenDecimals),
                validUntil: request.Context.StrapiProjectInfo!.CurrentPhase!.Finish!.Value,
                nonce: request.Context.UserInvestments!.Length
            )
        );
}
