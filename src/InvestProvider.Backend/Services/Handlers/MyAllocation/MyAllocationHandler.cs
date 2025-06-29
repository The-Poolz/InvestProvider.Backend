using MediatR;
using InvestProvider.Backend.Services.Handlers.MyAllocation.Models;

namespace InvestProvider.Backend.Services.Handlers.MyAllocation;

public class MyAllocationHandler : IRequestHandler<MyAllocationRequest, MyAllocationResponse>
{
    public Task<MyAllocationResponse> Handle(MyAllocationRequest request, CancellationToken cancellationToken)
    {
        var response = new MyAllocationResponse(
            amount: request.WhiteList?.Amount ?? (decimal)request.StrapiProjectInfo.CurrentPhase!.MaxInvest!,
            startTime: request.StrapiProjectInfo.CurrentPhase!.Start!.Value,
            endTime: request.StrapiProjectInfo.CurrentPhase.Finish!.Value,
            poolzBackId: request.DynamoDbProjectsInfo.PoolzBackId
        );
        return Task.FromResult(response);
    }
}