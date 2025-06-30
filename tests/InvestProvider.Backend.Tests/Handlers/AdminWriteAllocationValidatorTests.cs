using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Moq;
using Xunit;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Handlers.AdminWriteAllocation;
using InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;
using Net.Web3.EthereumWallet;
using FluentValidation;
using InvestProvider.Backend.Services.Handlers.ContextBuilders;

namespace InvestProvider.Backend.Tests.Handlers;

public class AdminWriteAllocationValidatorTests
{

    [Fact]
    public async Task Validate_Succeeds_ForWhitelistPhase()
    {
        var phase = TestHelpers.CreatePhase("1", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 0m);
        var phasesList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(phase.GetType()))!;
        phasesList.Add(phase);
        var projectInfo = TestHelpers.CreateProjectInfo(1, phasesList);

        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveProjectInfoAsync("pid", false)).ReturnsAsync(projectInfo);

        var dynamoDb = new Mock<IDynamoDBContext>();
        dynamoDb.Setup(x => x.LoadAsync<ProjectsInformation>("pid", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProjectsInformation { ProjectId = "pid", PoolzBackId = 5 });

        var builder = new PhaseContextBuilder<AdminWriteAllocationRequest>(strapi.Object, dynamoDb.Object);
        var validator = new AdminWriteAllocationValidator();
        var request = new AdminWriteAllocationRequest("pid", "1", new[] { new UserWithAmount(new EthereumAddress("0x0000000000000000000000000000000000000001"), 10) });

        await builder.BuildAsync(request, CancellationToken.None);
        await validator.ValidateAndThrowAsync(request);
    }

    [Fact]
    public async Task Validate_Throws_WhenActivePhaseMissing()
    {
        var type = Type.GetType("Poolz.Finance.CSharp.Strapi.ComponentPhaseStartEndAmount, Poolz.Finance.CSharp.Strapi")!;
        var emptyList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type))!;
        var projectInfo = TestHelpers.CreateProjectInfo(1, emptyList);

        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveProjectInfoAsync("pid", false)).ReturnsAsync(projectInfo);

        var dynamoDb = new Mock<IDynamoDBContext>();
        dynamoDb.Setup(x => x.LoadAsync<ProjectsInformation>("pid", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProjectsInformation { ProjectId = "pid", PoolzBackId = 5 });

        var builder = new PhaseContextBuilder<AdminWriteAllocationRequest>(strapi.Object, dynamoDb.Object);
        var validator = new AdminWriteAllocationValidator();
        var request = new AdminWriteAllocationRequest("pid", "1", Array.Empty<UserWithAmount>());

        await builder.BuildAsync(request, CancellationToken.None);
        await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAndThrowAsync(request));
    }
}
