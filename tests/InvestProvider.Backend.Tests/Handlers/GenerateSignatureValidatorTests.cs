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
using Net.Cache.DynamoDb.ERC20.Models;
using Nethereum.RPC.Eth.DTOs;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.Web3;
using poolz.finance.csharp.contracts.LockDealNFT;
using poolz.finance.csharp.contracts.InvestProvider;
using poolz.finance.csharp.contracts.InvestProvider.ContractDefinition;
using InvestProvider.Backend.Services.Handlers.GenerateSignature;
using InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

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

    private static void SetupCommonMocks(Mock<ILockDealNFTService<ContractType>> lockDealNFT, Mock<ERC20CacheProvider> erc20,
        Mock<IRpcProvider> rpcProvider, Mock<IInvestProviderService<ContractType>> investProvider)
    {
        lockDealNFT.Setup(x => x.TokenOfQueryAsync(
                It.IsAny<long>(),
                ContractType.LockDealNFT,
                It.IsAny<BigInteger>(),
                It.IsAny<BlockParameter>()))
                   .ReturnsAsync(new EthereumAddress("0x00000000000000000000000000000000000000aa"));
        rpcProvider.Setup(x => x.RpcUrl(It.IsAny<long>())).Returns("http://rpc");
        investProvider.Setup(x => x.GetUserInvestsQueryAsync(
                It.IsAny<long>(),
                ContractType.InvestedProvider,
                It.IsAny<BigInteger>(),
                It.IsAny<string>(),
                It.IsAny<BlockParameter>()))
                        .ReturnsAsync(new GetUserInvestsOutputDTO { ReturnValue1 = [] });
        erc20.Setup(x => x.GetOrAdd(It.IsAny<GetCacheRequest>()))
             .Returns(new ERC20DynamoDbTable { Decimals = (byte)18 });
    }

    [Fact]
    public async Task Validate_Succeeds_ForWhitelistPhase()
    {
        var phase = TestHelpers.CreatePhase("1", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 0m);
        var projectInfo = TestHelpers.CreateProjectInfo(1, phase);

        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveProjectInfoAsync("pid", It.IsAny<bool>())).ReturnsAsync(projectInfo);

        var dynamoDb = new Mock<IDynamoDBContext>();
        Environment.SetEnvironmentVariable("AWS_REGION", "us-east-1");
        var start = (DateTime)((dynamic)phase).Start;
        dynamoDb.Setup(x => x.LoadAsync<ProjectsInformation>("pid", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProjectsInformation { ProjectId = "pid", PoolzBackId = 5 });
        dynamoDb.Setup(x => x.LoadAsync<WhiteList>(WhiteList.CalculateHashId("pid", start), "0x0000000000000000000000000000000000000123", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new WhiteList("pid", start, new EthereumAddress("0x0000000000000000000000000000000000000123"), 10));

        var lockDealNFT = new Mock<ILockDealNFTService<ContractType>>();
        var erc20 = new Mock<ERC20CacheProvider>();
        var rpcProvider = new Mock<IRpcProvider>();
        var investProvider = new Mock<IInvestProviderService<ContractType>>();
        SetupCommonMocks(lockDealNFT, erc20, rpcProvider, investProvider);

        var validator = new GenerateSignatureRequestValidator(strapi.Object, dynamoDb.Object, rpcProvider.Object, erc20.Object, lockDealNFT.Object, investProvider.Object);
        var request = CreateRequest(projectInfo);

        await validator.ValidateAndThrowAsync(request);
    }

    [Fact]
    public async Task Validate_Throws_WhenAmountLessThanMinimum()
    {
        var phase = TestHelpers.CreatePhase("1", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 0m);
        var projectInfo = TestHelpers.CreateProjectInfo(1, phase);

        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveProjectInfoAsync("pid", It.IsAny<bool>())).ReturnsAsync(projectInfo);

        var dynamoDb = new Mock<IDynamoDBContext>();
        var start = (DateTime)((dynamic)phase).Start;
        dynamoDb.Setup(x => x.LoadAsync<ProjectsInformation>("pid", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProjectsInformation { ProjectId = "pid", PoolzBackId = 5 });
        Environment.SetEnvironmentVariable("AWS_REGION", "us-east-1");
        dynamoDb.Setup(x => x.LoadAsync<WhiteList>(WhiteList.CalculateHashId("pid", start), "0x0000000000000000000000000000000000000123", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new WhiteList("pid", start, new EthereumAddress("0x0000000000000000000000000000000000000123"), 10));

        var lockDealNFT = new Mock<ILockDealNFTService<ContractType>>();
        var erc20 = new Mock<ERC20CacheProvider>();
        var rpcProvider = new Mock<IRpcProvider>();
        var investProvider = new Mock<IInvestProviderService<ContractType>>();
        SetupCommonMocks(lockDealNFT, erc20, rpcProvider, investProvider);

        var validator = new GenerateSignatureRequestValidator(strapi.Object, dynamoDb.Object, rpcProvider.Object, erc20.Object, lockDealNFT.Object, investProvider.Object);
        var request = new GenerateSignatureRequest("pid", new EthereumAddress("0x0000000000000000000000000000000000000123"), "500000000000000000")
        {
            StrapiProjectInfo = projectInfo,
            DynamoDbProjectsInfo = new ProjectsInformation { ProjectId = "pid", PoolzBackId = 5 }
        };

        await Assert.ThrowsAsync<ValidationException>(() => validator.ValidateAndThrowAsync(request));
    }
}
