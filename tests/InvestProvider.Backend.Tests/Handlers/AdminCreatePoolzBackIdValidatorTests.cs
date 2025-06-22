using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Nethereum.RPC.Eth.DTOs;
using Moq;
using Xunit;
using FluentValidation;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId;
using InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId.Models;
using InvestProvider.Backend.Services.Web3.Contracts;
using poolz.finance.csharp.contracts.LockDealNFT;
using poolz.finance.csharp.contracts.LockDealNFT.ContractDefinition;
using InvestProvider.Backend.Tests;

namespace InvestProvider.Backend.Tests.Handlers;

public class AdminCreatePoolzBackIdValidatorTests
{
    [Fact]
    public async Task Validate_Succeeds_ForInvestProviderPool()
    {
        var phase = TestHelpers.CreatePhase("1", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 0m);
        var projectInfo = TestHelpers.CreateProjectInfo(1, phase);

        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveProjectInfoAsync("pid", false)).ReturnsAsync(projectInfo);

        var lockDealNFT = new Mock<ILockDealNFTService<ContractType>>();
        var poolInfo = new List<BasePoolInfo>
        {
            new BasePoolInfo { Name = ContractNames.InvestProvider },
            new BasePoolInfo { Name = ContractNames.DispenserProvider }
        };
        var fullData = new GetFullDataOutputDTO { PoolInfo = poolInfo };
        lockDealNFT.Setup(x => x.GetFullDataQueryAsync(1, ContractType.LockDealNFT, It.IsAny<BigInteger>(), It.IsAny<BlockParameter>()))
                   .ReturnsAsync(fullData);

        var dynamoDb = new Mock<IDynamoDBContext>();
        var validator = new AdminCreatePoolzBackIdValidator(strapi.Object, dynamoDb.Object, lockDealNFT.Object);
        var request = new AdminCreatePoolzBackIdRequest
        {
            ProjectId = "pid",
            PoolzBackId = 5,
            ChainId = 1
        };

        await validator.ValidateAndThrowAsync(request);
    }

    [Fact]
    public async Task Validate_Throws_WhenPoolTypeInvalid()
    {
        var phase = TestHelpers.CreatePhase("1", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 0m);
        var projectInfo = TestHelpers.CreateProjectInfo(1, phase);

        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveProjectInfoAsync("pid", false)).ReturnsAsync(projectInfo);

        var lockDealNFT = new Mock<ILockDealNFTService<ContractType>>();
        var poolInfo = new List<BasePoolInfo>
        {
            new BasePoolInfo { Name = "Other" }
        };
        var fullData = new GetFullDataOutputDTO { PoolInfo = poolInfo };
        lockDealNFT.Setup(x => x.GetFullDataQueryAsync(1, ContractType.LockDealNFT, It.IsAny<BigInteger>(), It.IsAny<BlockParameter>()))
                   .ReturnsAsync(fullData);

        var dynamoDb = new Mock<IDynamoDBContext>();
        var validator = new AdminCreatePoolzBackIdValidator(strapi.Object, dynamoDb.Object, lockDealNFT.Object);
        var request = new AdminCreatePoolzBackIdRequest
        {
            ProjectId = "pid",
            PoolzBackId = 5,
            ChainId = 1
        };

        await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAndThrowAsync(request));
    }
}
