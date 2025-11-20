using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Moq;
using Xunit;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Handlers.AdminGetAllocation;
using InvestProvider.Backend.Services.Handlers.AdminGetAllocation.Models;
using InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;
using InvestProvider.Backend.Services.Strapi;
using Net.Web3.EthereumWallet;

namespace InvestProvider.Backend.Tests.Handlers;

public class AdminGetAllocationHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsWhiteLists_ForAllPhases()
    {
        var phase1 = TestHelpers.CreatePhase("1", DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(-1), 0m);
        var phase2 = TestHelpers.CreatePhase("2", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 0m);
        var phases = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(phase1.GetType()))!;
        phases.Add(phase1);
        phases.Add(phase2);
        var projectInfo = TestHelpers.CreateProjectInfo(1, phases);

        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveProjectInfoAsync("pid", false)).ReturnsAsync(projectInfo);

        var dynamoDb = new Mock<IDynamoDBContext>();
        var start1 = (DateTime)((dynamic)phase1).Start;
        var start2 = (DateTime)((dynamic)phase2).Start;
        var whiteLists1 = new List<WhiteList>
        {
            new("pid", start1, new EthereumAddress("0x0000000000000000000000000000000000000001"), 1),
        };
        var whiteLists2 = new List<WhiteList>
        {
            new("pid", start2, new EthereumAddress("0x0000000000000000000000000000000000000002"), 2),
            new("pid", start2, new EthereumAddress("0x0000000000000000000000000000000000000003"), 3),
        };
        var hash1 = WhiteList.CalculateHashId("pid", start1);
        var hash2 = WhiteList.CalculateHashId("pid", start2);
        dynamoDb.Setup(x => x.QueryAsync<WhiteList>(hash1))
                .Returns(new StubAsyncSearch<WhiteList>(whiteLists1));
        dynamoDb.Setup(x => x.QueryAsync<WhiteList>(hash2))
                .Returns(new StubAsyncSearch<WhiteList>(whiteLists2));

        var handler = new AdminGetAllocationHandler(strapi.Object, dynamoDb.Object);
        var request = new AdminGetAllocationRequest("pid");

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(2, result.Count);
        var phase1Result = result.First(r => r.PhaseId == "1");
        Assert.Single(phase1Result.WhiteList);
        var phase2Result = result.First(r => r.PhaseId == "2");
        Assert.Equal(2, phase2Result.WhiteList.Count);
    }

    [Fact]
    public async Task Handle_FiltersOutZeroAmountEntries()
    {
        var phase = TestHelpers.CreatePhase("1", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 0m);
        var phases = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(phase.GetType()))!;
        phases.Add(phase);
        var projectInfo = TestHelpers.CreateProjectInfo(1, phases);

        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveProjectInfoAsync("pid", false)).ReturnsAsync(projectInfo);

        var dynamoDb = new Mock<IDynamoDBContext>();
        var start = (DateTime)((dynamic)phase).Start;
        var whiteLists = new List<WhiteList>
        {
            new("pid", start, new EthereumAddress("0x0000000000000000000000000000000000000001"), 0),
            new("pid", start, new EthereumAddress("0x0000000000000000000000000000000000000002"), 5),
        };
        var hash = WhiteList.CalculateHashId("pid", start);
        dynamoDb.Setup(x => x.QueryAsync<WhiteList>(hash))
                .Returns(new StubAsyncSearch<WhiteList>(whiteLists));

        var handler = new AdminGetAllocationHandler(strapi.Object, dynamoDb.Object);
        var request = new AdminGetAllocationRequest("pid");

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Single(result);
        var phaseResult = result.First();
        Assert.Single(phaseResult.WhiteList);
        Assert.Equal("0x0000000000000000000000000000000000000002", phaseResult.WhiteList.First().UserAddress.Address);
    }
}
