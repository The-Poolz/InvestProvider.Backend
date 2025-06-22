using MediatR;
using InvestProvider.Backend.Services.Handlers.MyAllocation.Models;

namespace InvestProvider.Backend.Services.Handlers.MyAllocation;

public class MyAllocationHandler
    : IRequestHandler<MyAllocationRequest, MyAllocationResponse>
{
    public Task<MyAllocationResponse> Handle(MyAllocationRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new MyAllocationResponse(
            amount: request.PhaseContext.WhiteList.Amount,
            startTime: request.PhaseContext.StrapiProjectInfo.CurrentPhase!.Start!.Value,
            endTime: request.PhaseContext.StrapiProjectInfo.CurrentPhase.Finish!.Value,
            poolzBackId: request.PhaseContext.DynamoDbProjectsInfo.PoolzBackId
        ));
    }
}