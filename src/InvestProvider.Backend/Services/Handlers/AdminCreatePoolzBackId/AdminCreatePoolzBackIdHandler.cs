using MediatR;
using Net.Cache.DynamoDb.ERC20;
using Amazon.DynamoDBv2.DataModel;
using Net.Cache.DynamoDb.ERC20.Models;
using InvestProvider.Backend.Services.Strapi;
using poolz.finance.csharp.contracts.LockDealNFT;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId;

public class AdminCreatePoolzBackIdHandler(
    IDynamoDBContext dynamoDb,
    ILockDealNFTService<ContractType> lockDealNFT,
    IStrapiClient strapi,
    ERC20CacheProvider erc20Cache
)
    : IRequestHandler<AdminCreatePoolzBackIdRequest, AdminCreatePoolzBackIdResponse>
{
    public async Task<AdminCreatePoolzBackIdResponse> Handle(AdminCreatePoolzBackIdRequest request, CancellationToken cancellationToken)
    {
        var token = await lockDealNFT
            .TokenOfQueryAsync(request.ChainId, ContractType.LockDealNFT, request.PoolzBackId)
            .ConfigureAwait(false);

        var onChainInfo = await strapi.ReceiveOnChainInfoAsync(request.ChainId);

        _ = erc20Cache.GetOrAdd(new GetCacheRequest(request.ChainId, token, onChainInfo.RpcUrl));

        await dynamoDb.SaveAsync(request, cancellationToken);

        return new AdminCreatePoolzBackIdResponse(request);
    }
}