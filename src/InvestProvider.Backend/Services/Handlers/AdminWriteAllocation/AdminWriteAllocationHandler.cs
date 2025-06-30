using MediatR;
using Amazon.DynamoDBv2.DataModel;
using Net.Web3.EthereumWallet.Extensions;
using EnvironmentManager.Extensions;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminWriteAllocation;

public class AdminWriteAllocationHandler(IDynamoDBContext dynamoDb)
    : IRequestHandler<AdminWriteAllocationRequest, AdminWriteAllocationResponse>
{
    private static readonly int BatchSize = Env.BATCH_SIZE.GetOrDefault(25);
    private static readonly int MaxParallel = Env.MAX_PARALLEL.GetOrDefault(10);

    public async Task<AdminWriteAllocationResponse> Handle(AdminWriteAllocationRequest request, CancellationToken cancellationToken)
    {
        var ctx = request.Context;
        var toSave = request.Users.Select(x => new WhiteList(request.ProjectId, ctx.Phase!.Start!.Value, x.UserAddress.ConvertToChecksumAddress(), x.Amount)).ToArray();
        await Parallel.ForEachAsync(toSave.Chunk(BatchSize), new ParallelOptions 
            {
                MaxDegreeOfParallelism = MaxParallel,
                CancellationToken = cancellationToken
            },
            body: async (chunk, ct) =>
            {
                var batch = dynamoDb.CreateBatchWrite<WhiteList>();
                batch.AddPutItems(chunk);
                await batch.ExecuteAsync(ct);
            }
        );

        return new AdminWriteAllocationResponse(toSave.Length);
    }
}