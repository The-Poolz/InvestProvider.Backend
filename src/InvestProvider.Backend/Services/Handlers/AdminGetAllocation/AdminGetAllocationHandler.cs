using MediatR;
using Amazon.DynamoDBv2.DataModel;
using System.Collections.Concurrent;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Handlers.AdminGetAllocation.Models;
using InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminGetAllocation;

public class AdminGetAllocationHandler(IStrapiClient strapi, IDynamoDBContext dynamoDb)
    : IRequestHandler<AdminGetAllocationRequest, ICollection<AdminGetAllocationResponse>>
{
    public int MaxParallel = 10;

    public async Task<ICollection<AdminGetAllocationResponse>> Handle(AdminGetAllocationRequest request, CancellationToken cancellationToken)
    {
        var projectInfo = strapi.ReceiveProjectInfo(request.ProjectId);

        var throttler = new SemaphoreSlim(MaxParallel);
        var whiteListPhases = projectInfo.Phases.Where(x => x.MaxInvest == 0);
        var whiteLists = new ConcurrentBag<AdminGetAllocationResponse>();
        var tasks = whiteListPhases.Select(async phase =>
        {
            await throttler.WaitAsync(cancellationToken);
            try
            {
                var search = dynamoDb.QueryAsync<WhiteList>(WhiteList.CalculateHashId(request.ProjectId, phase.Start!.Value));
                var entities = await search.GetRemainingAsync(cancellationToken);
                whiteLists.Add(new AdminGetAllocationResponse(
                    phaseId: phase.Id,
                    whiteList: entities.Select(x => new UserWithAmount(x.UserAddress, x.Amount)).ToArray()
                ));
            }
            finally
            {
                throttler.Release();
            }
        });
        
        await Task.WhenAll(tasks);

        return whiteLists.ToArray();
    }
}