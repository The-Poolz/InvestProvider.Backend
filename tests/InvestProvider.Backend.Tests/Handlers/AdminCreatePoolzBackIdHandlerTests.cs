using Moq;
using Xunit;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.RPC.Eth.DTOs;
using Net.Web3.EthereumWallet;
using Net.Cache.DynamoDb.ERC20;
using Amazon.DynamoDBv2.DataModel;
using Net.Cache.DynamoDb.ERC20.Models;
using InvestProvider.Backend.Services.Strapi;
using poolz.finance.csharp.contracts.LockDealNFT;
using InvestProvider.Backend.Services.Strapi.Models;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId;
using InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId.Models;

namespace InvestProvider.Backend.Tests.Handlers;

public class AdminCreatePoolzBackIdHandlerTests
{
    [Fact]
    public async Task Handle_SavesItem_AndReturnsResponse()
    {
        Environment.SetEnvironmentVariable("AWS_REGION", "us-east-1");

        var phase = TestHelpers.CreatePhase("1", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 0m);
        var projectInfo = TestHelpers.CreateProjectInfo(1, phase);
        var dynamoDb = new Mock<IDynamoDBContext>();
        var lockDealNFT = new Mock<ILockDealNFTService<ContractType>>();
        lockDealNFT.Setup(x => x.TokenOfQueryAsync(It.IsAny<long>(), ContractType.LockDealNFT, It.IsAny<BigInteger>(), It.IsAny<BlockParameter>()))
            .ReturnsAsync(new EthereumAddress("0x00000000000000000000000000000000000000aa"));

        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveOnChainInfoAsync(1))
            .ReturnsAsync(new OnChainInfo(
                "http://rpc",
                new EthereumAddress("0x00000000000000000000000000000000000000bb"),
                new EthereumAddress("0x00000000000000000000000000000000000000cc")
            ));

        var erc20 = new Mock<ERC20CacheProvider>();
        erc20.Setup(x => x.GetOrAdd(It.IsAny<GetCacheRequest>()))
            .Returns(new ERC20DynamoDbTable());

        var handler = new AdminCreatePoolzBackIdHandler(
            dynamoDb.Object,
            lockDealNFT.Object,
            strapi.Object,
            erc20.Object);
        var request = new AdminCreatePoolzBackIdRequest
        {
            ProjectId = "pid",
            PoolzBackId = 42,
            ChainId = 1,
            StrapiProjectInfo = projectInfo
        };

        var result = await handler.Handle(request, CancellationToken.None);

        dynamoDb.Verify(x => x.SaveAsync(request, CancellationToken.None), Times.Once);
        Assert.Equal("pid", result.ProjectId);
        Assert.Equal(42, result.PoolzBackId);
    }
}
