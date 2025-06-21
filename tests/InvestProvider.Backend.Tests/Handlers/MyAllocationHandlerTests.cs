using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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

namespace InvestProvider.Backend.Tests.Handlers;

public class MyAllocationHandlerTests
{
    private static object CreatePhase(string id, DateTime start, DateTime finish, decimal maxInvest)
    {
        var type = Type.GetType("Poolz.Finance.CSharp.Strapi.ComponentPhaseStartEndAmount, Poolz.Finance.CSharp.Strapi")!;
        var obj = Activator.CreateInstance(type)!;
        type.GetProperty("Id")?.SetValue(obj, id);
        type.GetProperty("Start")?.SetValue(obj, (DateTime?)start);
        type.GetProperty("Finish")?.SetValue(obj, (DateTime?)finish);
        var maxInvestProp = type.GetProperty("MaxInvest");
        if (maxInvestProp != null)
        {
            var targetType = Nullable.GetUnderlyingType(maxInvestProp.PropertyType) ?? maxInvestProp.PropertyType;
            object? converted = Convert.ChangeType(maxInvest, targetType);
            maxInvestProp.SetValue(obj, converted);
        }
        return obj;
    }

    private static ProjectInfo CreateProjectInfo(long chainId, object phase)
    {
        var projectsInfoType = Type.GetType("Poolz.Finance.CSharp.Strapi.ProjectsInformation, Poolz.Finance.CSharp.Strapi")!;
        var chainSettingType = Type.GetType("Poolz.Finance.CSharp.Strapi.ChainSetting, Poolz.Finance.CSharp.Strapi")!;
        var chainType = Type.GetType("Poolz.Finance.CSharp.Strapi.Chain, Poolz.Finance.CSharp.Strapi")!;

        var phasesList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(phase.GetType()))!;
        phasesList.Add(phase);

        var chain = Activator.CreateInstance(chainType)!;
        chainType.GetProperty("ChainId")?.SetValue(chain, (long?)chainId);

        var chainSetting = Activator.CreateInstance(chainSettingType)!;
        chainSettingType.GetProperty("Chain")?.SetValue(chainSetting, chain);

        var projectsInfo = Activator.CreateInstance(projectsInfoType)!;
        projectsInfoType.GetProperty("ChainSetting")?.SetValue(projectsInfo, chainSetting);
        projectsInfoType.GetProperty("ProjectPhases")?.SetValue(projectsInfo, phasesList);

        var projectInfoResponse = new ProjectInfoResponse((dynamic)projectsInfo);
        return new ProjectInfo(projectInfoResponse);
    }

    [Fact]
    public async Task Handle_ReturnsResponse_WhenDataExists()
    {
        var phase = CreatePhase("1", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 0m);
        var projectInfo = CreateProjectInfo(1, phase);

        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveProjectInfo("pid", false)).Returns(projectInfo);

        var dynamoDb = new Mock<IDynamoDBContext>();
        var projectData = new ProjectsInformation { ProjectId = "pid", PoolzBackId = 5 };
        dynamoDb.Setup(x => x.LoadAsync<ProjectsInformation>("pid", It.IsAny<CancellationToken>()))
                .ReturnsAsync(projectData);

        var address = new EthereumAddress("0x123");
        var startTime = (DateTime)((dynamic)phase).Start;
        var whiteList = new WhiteList("pid", startTime, address, 10);
        dynamoDb.Setup(x => x.LoadAsync<WhiteList>(WhiteList.CalculateHashId("pid", startTime), address.Address, It.IsAny<CancellationToken>()))
                .ReturnsAsync(whiteList);

        var handler = new MyAllocationHandler(strapi.Object, dynamoDb.Object);
        var request = new MyAllocationRequest("pid", address);

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(whiteList.Amount, result.Amount);
        Assert.Equal(((dynamic)phase).Start, result.StartTime);
        Assert.Equal(((dynamic)phase).Finish, result.EndTime);
        Assert.Equal(projectData.PoolzBackId, result.PoolzBackId);
    }

    [Fact]
    public async Task Handle_Throws_WhenWhiteListMissing()
    {
        var phase = CreatePhase("2", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 0m);
        var projectInfo = CreateProjectInfo(1, phase);

        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveProjectInfo("pid", false)).Returns(projectInfo);

        var dynamoDb = new Mock<IDynamoDBContext>();
        var projectData = new ProjectsInformation { ProjectId = "pid", PoolzBackId = 5 };
        dynamoDb.Setup(x => x.LoadAsync<ProjectsInformation>("pid", It.IsAny<CancellationToken>()))
                .ReturnsAsync(projectData);
        dynamoDb.Setup(x => x.LoadAsync<WhiteList>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<WhiteList?>(null));

        var handler = new MyAllocationHandler(strapi.Object, dynamoDb.Object);
        var request = new MyAllocationRequest("pid", new EthereumAddress("0x123"));

        var ex = await Assert.ThrowsAsync<Exception>(() => handler.Handle(request, CancellationToken.None));
        Assert.Contains("User not in white list", ex.Message);
    }
}
