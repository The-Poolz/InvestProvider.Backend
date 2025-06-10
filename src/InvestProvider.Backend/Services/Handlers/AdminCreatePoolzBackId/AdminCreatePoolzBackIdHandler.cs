using MediatR;
using Amazon.DynamoDBv2.DataModel;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Strapi;
using poolz.finance.csharp.contracts.LockDealNFT;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId;

public class AdminCreatePoolzBackIdHandler(
    ILockDealNFTService<ContractType> lockDealNFT,
    IDynamoDBContext dynamoDb,
    IStrapiClient strapi
)
    : IRequestHandler<AdminCreatePoolzBackIdRequest, AdminCreatePoolzBackIdResponse>
{
    public async Task<AdminCreatePoolzBackIdResponse> Handle(AdminCreatePoolzBackIdRequest request, CancellationToken cancellationToken)
    {
        _ = strapi.ReceiveProjectInfo(request.ProjectId);

        var getFullData = await lockDealNFT.GetFullDataQueryAsync(request.ChainId, ContractType.LockDealNFT, request.PoolzBackId);
        if (getFullData.PoolInfo.Count != 2 ||
            getFullData.PoolInfo[0].Name != StrapiClient.NameOfInvestedProvider ||
            getFullData.PoolInfo[1].Name != StrapiClient.NameOfDispenserProvider
        ) throw Error.INVALID_POOL_TYPE.ToException();

        await dynamoDb.SaveAsync(request, cancellationToken);

        return new AdminCreatePoolzBackIdResponse(request);
    }
}