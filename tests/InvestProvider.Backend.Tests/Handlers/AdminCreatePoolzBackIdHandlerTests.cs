using Moq;
using Xunit;
using System;
using Nethereum.Web3;
using System.Net.Http;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.RPC.Eth.DTOs;
using System.Net.Http.Headers;
using Net.Web3.EthereumWallet;
using Net.Cache.DynamoDb.ERC20;
using Poolz.Finance.CSharp.Http;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using InvestProvider.Backend.Services.Web3;
using InvestProvider.Backend.Services.Strapi;
using Net.Cache.DynamoDb.ERC20.DynamoDb.Models;
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
        using var _ = EnvironmentVariableScope.Set(new Dictionary<string, string?>
        {
            ["AWS_REGION"] = "us-east-1",
            ["BASE_URL_OF_RPC"] = "http://rpc",
            [nameof(Env.MULTI_CALL_V3_ADDRESS)] = "0x0000000000000000000000000000000000000000"
        });

        var phase = TestHelpers.CreatePhase("1", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 0m);
        var projectInfo = TestHelpers.CreateProjectInfo(1, phase);
        var dynamoDb = new Mock<IDynamoDBContext>();
        var lockDealNFT = new Mock<ILockDealNFTService<ContractType>>();
        lockDealNFT.Setup(x => x.TokenOfQueryAsync(It.IsAny<long>(), ContractType.LockDealNFT, It.IsAny<BigInteger>(), It.IsAny<BlockParameter>()))
            .ReturnsAsync(new EthereumAddress("0x00000000000000000000000000000000000000aa"));

        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveOnChainInfoAsync(1))
            .ReturnsAsync(new OnChainInfo(
                new EthereumAddress("0x00000000000000000000000000000000000000bb"),
                new EthereumAddress("0x00000000000000000000000000000000000000cc")
            ));

        var erc20 = new Mock<IErc20CacheService>();
        erc20.Setup(x => x.GetOrAddAsync(
                It.IsAny<HashKey>(),
                It.IsAny<Func<Task<IWeb3>>>(),
                It.IsAny<Func<Task<EthereumAddress>>>()
            ))
            .ReturnsAsync(new Erc20TokenDynamoDbEntry { HashKey = "hash" });

        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<Action<HttpRequestHeaders>>())).Returns(new HttpClient());

        var chainProvider = new ChainProvider(strapi.Object, httpClientFactory.Object);

        var handler = new AdminCreatePoolzBackIdHandler(dynamoDb.Object, lockDealNFT.Object, erc20.Object, chainProvider);
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
