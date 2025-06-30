using MediatR;
using InvestProvider.Backend.Services.Handlers.MyAllocation.Models;

namespace InvestProvider.Backend.Services.Handlers.MyAllocation;

public class MyAllocationHandler : IRequestHandler<MyAllocationRequest, MyAllocationResponse>
{
    public Task<MyAllocationResponse> Handle(MyAllocationRequest request, CancellationToken cancellationToken)
    {
        var ctx = request.Context;
        var response = new MyAllocationResponse(
            amount: ctx.WhiteList?.Amount ?? (decimal)ctx.StrapiProjectInfo!.CurrentPhase!.MaxInvest!,
            startTime: ctx.StrapiProjectInfo!.CurrentPhase!.Start!.Value,
            endTime: ctx.StrapiProjectInfo!.CurrentPhase!.Finish!.Value,
            poolzBackId: ctx.DynamoDbProjectsInfo!.PoolzBackId
        );
        return Task.FromResult(response);
    }
}