using MediatR;
using Amazon.DynamoDBv2.DataModel;
using Net.Web3.EthereumWallet.Extensions;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminWriteAllocation;

public class AdminWriteAllocationHandler(IDynamoDBContext dynamoDb)
    : IRequestHandler<AdminWriteAllocationRequest, AdminWriteAllocationResponse>
{
    private const int BatchSize = 25;
    private const int MaxParallel = 10;

    public async Task<AdminWriteAllocationResponse> Handle(AdminWriteAllocationRequest request, CancellationToken cancellationToken)
    {
        var toSave = request.Users.Select(x => new WhiteList(request.ProjectId, request.PhaseContext.Phase.Start!.Value, x.UserAddress.ConvertToChecksumAddress(), x.Amount)).ToArray();
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