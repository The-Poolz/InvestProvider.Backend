using MediatR;
using InvestProvider.Backend.Services.Handlers.MyAllocation.Models;
using System;

namespace InvestProvider.Backend.Services.Handlers.MyAllocation;

public class MyAllocationHandler
    : IRequestHandler<MyAllocationRequest, MyAllocationResponse>
{
    public Task<MyAllocationResponse> Handle(MyAllocationRequest request, CancellationToken cancellationToken)
    {
        var phase = request.StrapiProjectInfo.CurrentPhase!;
        decimal amount;
        var maxInvest = Convert.ToDecimal(phase.MaxInvest ?? 0);
        if (maxInvest == 0)
        {
            amount = request.WhiteList.Amount;
        }
        else
        {
            amount = request.UsedCurrentPhase ? 0m : maxInvest;
        }

        return Task.FromResult(new MyAllocationResponse(
            amount,
            phase.Start!.Value,
            phase.Finish!.Value,
            request.DynamoDbProjectsInfo.PoolzBackId
        ));
    }
}