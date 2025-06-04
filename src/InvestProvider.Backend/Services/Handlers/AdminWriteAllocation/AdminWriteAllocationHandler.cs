using MediatR;
using Amazon.DynamoDBv2.DataModel;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminWriteAllocation;

public class AdminWriteAllocationHandler(
    IDynamoDBContext dynamoDb,
    IStrapiClient strapi
)
    : IRequestHandler<AdminWriteAllocationRequest, AdminWriteAllocationResponse>
{
    private const int BatchSize = 25;
    private const int MaxParallel = 10;

    public async Task<AdminWriteAllocationResponse> Handle(AdminWriteAllocationRequest request, CancellationToken cancellationToken)
    {
        var dynamoProjectInfo = await dynamoDb.LoadAsync<ProjectsInformation>(request.ProjectId, cancellationToken);
        if (dynamoProjectInfo == null)
        {
            throw Error.POOLZ_BACK_ID_NOT_FOUND.ToException(new
            {
                request.ProjectId
            });
        }

        var projectInfo = strapi.ReceiveProjectInfo(request.ProjectId);
        var phase = projectInfo.Phases.FirstOrDefault(x => x.Id == request.PhaseId);
        if (phase == null)
        {
            throw Error.PHASE_IN_PROJECT_NOT_FOUND.ToException(new
            {
                request.ProjectId,
                request.PhaseId
            });
        }
        if (phase.MaxInvest != 0) throw Error.PHASE_IS_NOT_WHITELIST.ToException();

        // TODO: Check if not already finished

        await Parallel.ForEachAsync(request.ToSave.Chunk(BatchSize), new ParallelOptions 
            {
                MaxDegreeOfParallelism = MaxParallel,
                CancellationToken = cancellationToken
            },
            body: async (chunk, ct) =>
            {
                var batch = dynamoDb.CreateBatchWrite<UserData>();
                batch.AddPutItems(chunk);
                await batch.ExecuteAsync(ct);
            }
        );

        return new AdminWriteAllocationResponse
        {
            Saved = request.ToSave.Count
        };
    }
}