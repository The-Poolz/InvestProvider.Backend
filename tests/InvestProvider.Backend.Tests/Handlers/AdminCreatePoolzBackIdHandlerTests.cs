using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Amazon.DynamoDBv2.DataModel;
using InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId;
using InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId.Models;
using InvestProvider.Backend.Tests;

namespace InvestProvider.Backend.Tests.Handlers;

public class AdminCreatePoolzBackIdHandlerTests
{
    [Fact]
    public async Task Handle_SavesItem_AndReturnsResponse()
    {
        var phase = TestHelpers.CreatePhase("1", System.DateTime.UtcNow, System.DateTime.UtcNow.AddHours(1), 0m);
        var projectInfo = TestHelpers.CreateProjectInfo(1, phase);
        var dynamoDb = new Mock<IDynamoDBContext>();
        var handler = new AdminCreatePoolzBackIdHandler(dynamoDb.Object);
        var request = new AdminCreatePoolzBackIdRequest
        {
            ProjectId = "pid",
            PoolzBackId = 42,
            ChainId = 1
        };
        request.PhaseContext.StrapiProjectInfo = projectInfo;

        var result = await handler.Handle(request, CancellationToken.None);

        dynamoDb.Verify(x => x.SaveAsync(request, CancellationToken.None), Times.Once);
        Assert.Equal("pid", result.ProjectId);
        Assert.Equal(42, result.PoolzBackId);
    }
}
