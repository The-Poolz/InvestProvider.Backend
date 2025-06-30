using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Moq;
using Xunit;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Strapi.Models;
using InvestProvider.Backend.Services.Handlers.MyAllocation;
using InvestProvider.Backend.Services.Handlers.MyAllocation.Models;
using Net.Web3.EthereumWallet;
using FluentValidation;
using InvestProvider.Backend.Tests;
using InvestProvider.Backend.Services.Handlers.ContextBuilders;

namespace InvestProvider.Backend.Tests.Handlers;

public class MyAllocationHandlerTests
{

    [Fact]
    public async Task Handle_ReturnsResponse_WhenDataExists()
    {
        var phase = TestHelpers.CreatePhase("1", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 0m);
        var projectInfo = TestHelpers.CreateProjectInfo(1, phase);

        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveProjectInfoAsync("pid", true)).ReturnsAsync(projectInfo);

        var dynamoDb = new Mock<IDynamoDBContext>();
        var projectData = new ProjectsInformation { ProjectId = "pid", PoolzBackId = 5 };
        dynamoDb.Setup(x => x.LoadAsync<ProjectsInformation>("pid", It.IsAny<CancellationToken>()))
                .ReturnsAsync(projectData);

        var address = new EthereumAddress("0x0000000000000000000000000000000000000123");
        var startTime = (DateTime)((dynamic)phase).Start;
        var whiteList = new WhiteList("pid", startTime, address, 10);
        dynamoDb.Setup(x => x.LoadAsync<WhiteList>(WhiteList.CalculateHashId("pid", startTime), address.Address, It.IsAny<CancellationToken>()))
                .ReturnsAsync(whiteList);

        var builder = new PhaseContextBuilder<MyAllocationRequest>(strapi.Object, dynamoDb.Object);
        var validator = new MyAllocationValidator();
        var handler = new MyAllocationHandler();
        var request = new MyAllocationRequest("pid", address);

        await builder.BuildAsync(request, CancellationToken.None);
        await validator.ValidateAndThrowAsync(request);
        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(whiteList.Amount, result.Amount);
        Assert.Equal(((dynamic)phase).Start, result.StartTime);
        Assert.Equal(((dynamic)phase).Finish, result.EndTime);
        Assert.Equal(projectData.PoolzBackId, result.PoolzBackId);
    }

    [Fact]
    public async Task Handle_Throws_WhenWhiteListMissing()
    {
        var phase = TestHelpers.CreatePhase("2", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 0m);
        var projectInfo = TestHelpers.CreateProjectInfo(1, phase);

        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveProjectInfoAsync("pid", true)).ReturnsAsync(projectInfo);

        var dynamoDb = new Mock<IDynamoDBContext>();
        var projectData = new ProjectsInformation { ProjectId = "pid", PoolzBackId = 5 };
        dynamoDb.Setup(x => x.LoadAsync<ProjectsInformation>("pid", It.IsAny<CancellationToken>()))
                .ReturnsAsync(projectData);
        dynamoDb.Setup(x => x.LoadAsync<WhiteList>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<WhiteList>(null!));

        var builder = new PhaseContextBuilder<MyAllocationRequest>(strapi.Object, dynamoDb.Object);
        var validator = new MyAllocationValidator();
        var handler = new MyAllocationHandler();
        var request = new MyAllocationRequest("pid", new EthereumAddress("0x0000000000000000000000000000000000000123"));

        await builder.BuildAsync(request, CancellationToken.None);
        var ex = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () =>
        {
            await validator.ValidateAndThrowAsync(request);
            await handler.Handle(request, CancellationToken.None);
        });
        Assert.Contains("User not in white list", ex.Message);
    }
}
