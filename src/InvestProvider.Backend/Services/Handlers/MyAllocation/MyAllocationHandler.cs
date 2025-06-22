using MediatR;
using Amazon.DynamoDBv2.DataModel;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Handlers.MyAllocation.Models;

namespace InvestProvider.Backend.Services.Handlers.MyAllocation;

public class MyAllocationHandler(IStrapiClient strapi, IDynamoDBContext dynamoDb)
    : IRequestHandler<MyAllocationRequest, MyAllocationResponse>
{
    public async Task<MyAllocationResponse> Handle(MyAllocationRequest request, CancellationToken _)
    {
        var dynamoProjectInfo = await dynamoDb.LoadAsync<ProjectsInformation>(request.ProjectId);
        if (dynamoProjectInfo == null) throw Error.POOLZ_BACK_ID_NOT_FOUND.ToException(new
        {
            request.ProjectId
        });

        var projectInfo = strapi.ReceiveProjectInfo(request.ProjectId, filterPhases: false);
        if (projectInfo.CurrentPhase == null) throw Error.NOT_FOUND_ACTIVE_PHASE.ToException(new
        {
            request.ProjectId
        });
        if (projectInfo.CurrentPhase.MaxInvest!.Value != 0) throw Error.PHASE_IS_NOT_WHITELIST.ToException(new
        {
            request.ProjectId,
            PhaseId = projectInfo.CurrentPhase.Id
        });

        var whiteList = await dynamoDb.LoadAsync<WhiteList>(
            hashKey: WhiteList.CalculateHashId(request.ProjectId, projectInfo.CurrentPhase.Start!.Value),
            rangeKey: request.UserAddress.Address,
            CancellationToken.None
        );
        if (whiteList == null) throw Error.NOT_IN_WHITE_LIST.ToException(new
        {
            request.ProjectId,
            UserAddress = request.UserAddress.Address,
            PhaseId = projectInfo.CurrentPhase.Id
        });

        return new MyAllocationResponse(
            amount: whiteList.Amount,
            startTime: projectInfo.CurrentPhase.Start!.Value,
            endTime: projectInfo.CurrentPhase.Finish!.Value,
            poolzBackId: dynamoProjectInfo.PoolzBackId
        );
    }
}