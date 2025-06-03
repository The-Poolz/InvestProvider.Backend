using MediatR;
using Amazon.DynamoDBv2.DataModel;
using System.Collections.Concurrent;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Handlers.AdminGetAllocation.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminGetAllocation;

public class AdminGetAllocationHandler(IStrapiClient strapi, IDynamoDBContext dynamoDb) : IRequestHandler<AdminGetAllocationRequest, AdminGetAllocationResponse>
{
    public int MaxParallel = 10;

    public async Task<AdminGetAllocationResponse> Handle(AdminGetAllocationRequest request, CancellationToken cancellationToken)
    {
        // TODO: Here i can add filter into Strapi request if MaxInvest == 0, right?
        var projectInformation = strapi.ReceiveProjectInfo(request.ProjectId);

        var userData = new ConcurrentDictionary<string, IReadOnlyCollection<UserData>>();
        var throttler = new SemaphoreSlim(MaxParallel);
        var tasks = projectInformation.Phases.Select(x => x.Id).Select(async id =>
        {
            await throttler.WaitAsync(cancellationToken);
            try
            {
                var search = dynamoDb.QueryAsync<UserData>(id);
                var entities = await search.GetRemainingAsync(cancellationToken);
                userData.TryAdd(id, entities);
            }
            finally
            {
                throttler.Release();
            }
        });
        
        await Task.WhenAll(tasks);

        return new AdminGetAllocationResponse(userData);
    }
}