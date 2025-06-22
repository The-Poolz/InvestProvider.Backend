using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Moq;
using Xunit;
using InvestProvider.Backend.Services.Handlers.AdminWriteAllocation;
using InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;
using Net.Web3.EthereumWallet;
using InvestProvider.Backend.Tests;

namespace InvestProvider.Backend.Tests.Handlers;

public class AdminWriteAllocationHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsZero_WhenNoUsers()
    {
        var phase = TestHelpers.CreatePhase("1", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 0m);

        var dynamo = new Mock<IDynamoDBContext>(MockBehavior.Strict);
        var handler = new AdminWriteAllocationHandler(dynamo.Object);
        var request = new AdminWriteAllocationRequest("pid", "1", Array.Empty<UserWithAmount>());
        request.Phase = (dynamic)phase;

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(0, result.Saved);
        dynamo.VerifyNoOtherCalls();
    }
}
