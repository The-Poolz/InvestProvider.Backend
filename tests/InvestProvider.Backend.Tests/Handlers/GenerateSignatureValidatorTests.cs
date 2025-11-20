using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Moq;
using Xunit;
using FluentValidation;
using Net.Web3.EthereumWallet;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Strapi.Models;
using InvestProvider.Backend.Services.DynamoDb.Models;
using Net.Cache.DynamoDb.ERC20;
using Nethereum.RPC.Eth.DTOs;
using InvestProvider.Backend.Services.Web3.Contracts;
using poolz.finance.csharp.contracts.LockDealNFT;
using poolz.finance.csharp.contracts.InvestProvider;
using poolz.finance.csharp.contracts.InvestProvider.ContractDefinition;
using InvestProvider.Backend.Services.Handlers.GenerateSignature;
using InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;
using Net.Cache.DynamoDb.ERC20.DynamoDb.Models;
using Nethereum.Web3;
using NethereumGenerators.Interfaces;
using System.Collections.Generic;

namespace InvestProvider.Backend.Tests.Handlers;

public class GenerateSignatureValidatorTests
{
    private static GenerateSignatureRequest CreateRequest(ProjectInfo projectInfo)
    {
        return new GenerateSignatureRequest("pid", new EthereumAddress("0x0000000000000000000000000000000000000123"), "1000000000000000000")
        {
            StrapiProjectInfo = projectInfo,
            DynamoDbProjectsInfo = new ProjectsInformation { ProjectId = "pid", PoolzBackId = 5 }
        };
    }

    private static void SetupCommonMocks(
        Mock<ILockDealNFTService<ContractType>> lockDealNFT,
        Mock<IErc20CacheService> erc20,
        Mock<IChainProvider<ContractType>> chainProvider,
        Mock<IInvestProviderService<ContractType>> investProvider)
    {
        lockDealNFT.Setup(x => x.TokenOfQueryAsync(
                It.IsAny<long>(),
                ContractType.LockDealNFT,
                It.IsAny<BigInteger>(),
                It.IsAny<BlockParameter>()))
            .ReturnsAsync(new EthereumAddress("0x00000000000000000000000000000000000000aa"));
        chainProvider.Setup(x => x.Web3(It.IsAny<long>())).Returns(Mock.Of<IWeb3>());
        investProvider.Setup(x => x.GetUserInvestsQueryAsync(
                It.IsAny<long>(),
                ContractType.InvestedProvider,
                It.IsAny<BigInteger>(),
                It.IsAny<string>(),
                It.IsAny<BlockParameter>()))
            .ReturnsAsync(new GetUserInvestsOutputDTO { ReturnValue1 = [] });
        erc20.Setup(x => x.GetOrAddAsync(
                It.IsAny<HashKey>(),
                It.IsAny<Func<Task<IWeb3>>>(),
                It.IsAny<Func<Task<EthereumAddress>>>()
            ))
            .ReturnsAsync(new Erc20TokenDynamoDbEntry { Decimals = 18 });
    }

    [Fact]
    public async Task Validate_Succeeds_ForWhitelistPhase()
    {
        using var _ = EnvironmentVariableScope.Set(new Dictionary<string, string?>
        {
            ["AWS_REGION"] = "us-east-1",
            [nameof(Env.MULTI_CALL_V3_ADDRESS)] = "0x0000000000000000000000000000000000000000"
        });

        var phase = TestHelpers.CreatePhase("1", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 0m);
        var projectInfo = TestHelpers.CreateProjectInfo(1, phase);

        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveProjectInfoAsync("pid", It.IsAny<bool>())).ReturnsAsync(projectInfo);

        var dynamoDb = new Mock<IDynamoDBContext>();
        var start = (DateTime)((dynamic)phase).Start;
        dynamoDb.Setup(x => x.LoadAsync<ProjectsInformation>("pid", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProjectsInformation { ProjectId = "pid", PoolzBackId = 5 });
        dynamoDb.Setup(x => x.LoadAsync<WhiteList>(WhiteList.CalculateHashId("pid", start), "0x0000000000000000000000000000000000000123", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new WhiteList("pid", start, new EthereumAddress("0x0000000000000000000000000000000000000123"), 10));

        var lockDealNFT = new Mock<ILockDealNFTService<ContractType>>();
        var erc20 = new Mock<IErc20CacheService>();
        var chainProvider = new Mock<IChainProvider<ContractType>>();
        var investProvider = new Mock<IInvestProviderService<ContractType>>();
        SetupCommonMocks(lockDealNFT, erc20, chainProvider, investProvider);

        var validator = new GenerateSignatureRequestValidator(strapi.Object, dynamoDb.Object, chainProvider.Object, erc20.Object, lockDealNFT.Object, investProvider.Object);
        var request = CreateRequest(projectInfo);

        await validator.ValidateAndThrowAsync(request);
    }

    [Fact]
    public async Task Validate_Throws_WhenAmountLessThanMinimum()
    {
        using var _ = EnvironmentVariableScope.Set(new Dictionary<string, string?>
        {
            ["AWS_REGION"] = "us-east-1",
            [nameof(Env.MULTI_CALL_V3_ADDRESS)] = "0x0000000000000000000000000000000000000000"
        });

        var phase = TestHelpers.CreatePhase("1", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 0m);
        var projectInfo = TestHelpers.CreateProjectInfo(1, phase);

        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveProjectInfoAsync("pid", It.IsAny<bool>())).ReturnsAsync(projectInfo);

        var dynamoDb = new Mock<IDynamoDBContext>();
        var start = (DateTime)((dynamic)phase).Start;
        dynamoDb.Setup(x => x.LoadAsync<ProjectsInformation>("pid", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProjectsInformation { ProjectId = "pid", PoolzBackId = 5 });
        dynamoDb.Setup(x => x.LoadAsync<WhiteList>(WhiteList.CalculateHashId("pid", start), "0x0000000000000000000000000000000000000123", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new WhiteList("pid", start, new EthereumAddress("0x0000000000000000000000000000000000000123"), 10));

        var lockDealNFT = new Mock<ILockDealNFTService<ContractType>>();
        var erc20 = new Mock<IErc20CacheService>();
        var chainProvider = new Mock<IChainProvider<ContractType>>();
        var investProvider = new Mock<IInvestProviderService<ContractType>>();
        SetupCommonMocks(lockDealNFT, erc20, chainProvider, investProvider);

        var validator = new GenerateSignatureRequestValidator(strapi.Object, dynamoDb.Object, chainProvider.Object, erc20.Object, lockDealNFT.Object, investProvider.Object);
        var request = new GenerateSignatureRequest("pid", new EthereumAddress("0x0000000000000000000000000000000000000123"), "500000000000000000")
        {
            StrapiProjectInfo = projectInfo,
            DynamoDbProjectsInfo = new ProjectsInformation { ProjectId = "pid", PoolzBackId = 5 }
        };

        await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAndThrowAsync(request));
    }
}
