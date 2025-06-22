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
using InvestProvider.Backend.Services.Strapi.Models;
using InvestProvider.Backend.Services.Handlers.AdminWriteAllocation;
using InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;
using Net.Web3.EthereumWallet;
using FluentValidation;

namespace InvestProvider.Backend.Tests.Handlers;

public class AdminWriteAllocationValidatorTests
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

    private static ProjectInfo CreateProjectInfo(long chainId, IList phases)
    {
        var projectsInfoType = Type.GetType("Poolz.Finance.CSharp.Strapi.ProjectsInformation, Poolz.Finance.CSharp.Strapi")!;
        var chainSettingType = Type.GetType("Poolz.Finance.CSharp.Strapi.ChainSetting, Poolz.Finance.CSharp.Strapi")!;
        var chainType = Type.GetType("Poolz.Finance.CSharp.Strapi.Chain, Poolz.Finance.CSharp.Strapi")!;

        var chain = Activator.CreateInstance(chainType)!;
        chainType.GetProperty("ChainId")?.SetValue(chain, (long?)chainId);

        var chainSetting = Activator.CreateInstance(chainSettingType)!;
        chainSettingType.GetProperty("Chain")?.SetValue(chainSetting, chain);

        var projectsInfo = Activator.CreateInstance(projectsInfoType)!;
        projectsInfoType.GetProperty("ChainSetting")?.SetValue(projectsInfo, chainSetting);
        projectsInfoType.GetProperty("ProjectPhases")?.SetValue(projectsInfo, phases);

        var projectInfoResponse = new ProjectInfoResponse((dynamic)projectsInfo);
        return new ProjectInfo(projectInfoResponse);
    }

    [Fact]
    public async Task Validate_Succeeds_ForWhitelistPhase()
    {
        var phase = CreatePhase("1", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 0m);
        var phasesList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(phase.GetType()))!;
        phasesList.Add(phase);
        var projectInfo = CreateProjectInfo(1, phasesList);

        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveProjectInfo("pid", false)).Returns(projectInfo);

        var dynamoDb = new Mock<IDynamoDBContext>();
        dynamoDb.Setup(x => x.LoadAsync<ProjectsInformation>("pid", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProjectsInformation { ProjectId = "pid", PoolzBackId = 5 });

        var validator = new AdminWriteAllocationValidator(strapi.Object, dynamoDb.Object);
        var request = new AdminWriteAllocationRequest("pid", "1", new[] { new UserWithAmount(new EthereumAddress("0x0000000000000000000000000000000000000001"), 10) });

        await validator.ValidateAndThrowAsync(request);
    }

    [Fact]
    public async Task Validate_Throws_WhenActivePhaseMissing()
    {
        var type = Type.GetType("Poolz.Finance.CSharp.Strapi.ComponentPhaseStartEndAmount, Poolz.Finance.CSharp.Strapi")!;
        var emptyList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type))!;
        var projectInfo = CreateProjectInfo(1, emptyList);

        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveProjectInfo("pid", false)).Returns(projectInfo);

        var dynamoDb = new Mock<IDynamoDBContext>();
        dynamoDb.Setup(x => x.LoadAsync<ProjectsInformation>("pid", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProjectsInformation { ProjectId = "pid", PoolzBackId = 5 });

        var validator = new AdminWriteAllocationValidator(strapi.Object, dynamoDb.Object);
        var request = new AdminWriteAllocationRequest("pid", "1", Array.Empty<UserWithAmount>());

        await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAndThrowAsync(request));
    }
}
