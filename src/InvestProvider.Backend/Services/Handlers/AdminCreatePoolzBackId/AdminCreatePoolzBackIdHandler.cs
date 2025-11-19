using MediatR;
using Net.Web3.EthereumWallet;
using Net.Cache.DynamoDb.ERC20;
using Amazon.DynamoDBv2.DataModel;
using EnvironmentManager.Extensions;
using NethereumGenerators.Interfaces;
using Net.Cache.DynamoDb.ERC20.DynamoDb.Models;
using poolz.finance.csharp.contracts.LockDealNFT;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId;

public class AdminCreatePoolzBackIdHandler(
    IDynamoDBContext dynamoDb,
    ILockDealNFTService<ContractType> lockDealNFT,
    IErc20CacheService erc20Cache,
    IChainProvider<ContractType> chainProvider
)
    : IRequestHandler<AdminCreatePoolzBackIdRequest, AdminCreatePoolzBackIdResponse>
{
    public async Task<AdminCreatePoolzBackIdResponse> Handle(AdminCreatePoolzBackIdRequest request, CancellationToken cancellationToken)
    {
        var token = await lockDealNFT
            .TokenOfQueryAsync(request.ChainId, ContractType.LockDealNFT, request.PoolzBackId)
            .ConfigureAwait(false);

        var tokenInfo = await erc20Cache.GetOrAddAsync(
            new HashKey(request.ChainId, token),
            () => Task.FromResult(chainProvider.Web3(request.ChainId)),
            () => Task.FromResult<EthereumAddress>(Env.MULTI_CALL_V3_ADDRESS.GetRequired())
        );

        request.TokenHashKey = tokenInfo.HashKey;

        await dynamoDb.SaveAsync(request, cancellationToken);

        return new AdminCreatePoolzBackIdResponse(request);
    }
}