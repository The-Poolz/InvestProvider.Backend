using Moq;
using Xunit;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Net.Web3.EthereumWallet;
using Poolz.Finance.CSharp.Http;
using System.Collections.Generic;
using InvestProvider.Backend.Services.Web3;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Strapi.Models;
using InvestProvider.Backend.Services.Web3.Contracts;

namespace InvestProvider.Backend.Tests.Services;

public class ChainProviderTests
{
    [Fact]
    public void RpcUrl_IsFetched_FromStrapi_AndCached()
    {
        var baseRpcUrl = "http://rpc/evm/";
        using var _ = EnvironmentVariableScope.Set(new Dictionary<string, string?>
        {
            [nameof(Env.BASE_URL_OF_RPC)] = baseRpcUrl,
            [nameof(Env.MULTI_CALL_V3_ADDRESS)] = "0x0000000000000000000000000000000000000000"
        });
        var chainId = 56;

        var provider = new ChainProvider(Mock.Of<IStrapiClient>(), MockHttpClientFactory());

        Assert.Equal($"http://rpc/evm/{chainId}", ChainProvider.RpcUrl(chainId));
    }

    [Fact]
    public void ContractAddress_ReturnsAddress_ForKnownType()
    {
        var info = new OnChainInfo(new EthereumAddress("0x00000000000000000000000000000000000000AA"), new EthereumAddress("0x00000000000000000000000000000000000000bb"));
        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveOnChainInfoAsync(It.IsAny<long>())).ReturnsAsync(info);
        var provider = new ChainProvider(strapi.Object, MockHttpClientFactory());

        Assert.Equal("0x00000000000000000000000000000000000000AA", provider.ContractAddress(3, ContractType.InvestedProvider));
        Assert.Equal("0x00000000000000000000000000000000000000bb", provider.ContractAddress(3, ContractType.LockDealNFT));
    }

    [Fact]
    public void ContractAddress_Throws_ForUnsupportedType()
    {
        var info = new OnChainInfo(new EthereumAddress("0x00000000000000000000000000000000000000aa"), new EthereumAddress("0x00000000000000000000000000000000000000bb"));
        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveOnChainInfoAsync(4)).ReturnsAsync(info);
        var provider = new ChainProvider(Mock.Of<IStrapiClient>(), MockHttpClientFactory());

        Assert.ThrowsAny<Exception>(() => provider.ContractAddress(4, (ContractType)99));
    }

    [Fact]
    public void ContractAddress_CachesResponses_PerChain()
    {
        using var _ = EnvironmentVariableScope.Set(new Dictionary<string, string?>
        {
            [nameof(Env.BASE_URL_OF_RPC)] = "http://rpc/",
            [nameof(Env.MULTI_CALL_V3_ADDRESS)] = "0x0000000000000000000000000000000000000000"
        });

        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveOnChainInfoAsync(5))
            .ReturnsAsync(new OnChainInfo(
                new EthereumAddress("0x00000000000000000000000000000000000000AA"),
                new EthereumAddress("0x00000000000000000000000000000000000000bb")));

        var provider = new ChainProvider(strapi.Object, MockHttpClientFactory());

        var first = provider.ContractAddress(5, ContractType.InvestedProvider);
        var second = provider.ContractAddress(5, ContractType.LockDealNFT);

        Assert.Equal("0x00000000000000000000000000000000000000AA", first);
        Assert.Equal("0x00000000000000000000000000000000000000bb", second);
        strapi.Verify(x => x.ReceiveOnChainInfoAsync(5), Times.Once);
    }

    [Fact]
    public void Web3_IsCreated_WithRpcUrl()
    {
        var baseRpcUrl = "http://rpc/evm/";
        using var _ = EnvironmentVariableScope.Set(new Dictionary<string, string?>
        {
            [nameof(Env.BASE_URL_OF_RPC)] = baseRpcUrl,
            [nameof(Env.MULTI_CALL_V3_ADDRESS)] = "0x0000000000000000000000000000000000000000"
        });

        var createdForUrl = string.Empty;
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<Action<HttpRequestHeaders>>()))
            .Callback<string, Action<HttpRequestHeaders>>((url, _) => createdForUrl = url)
            .Returns(new HttpClient());

        var provider = new ChainProvider(Mock.Of<IStrapiClient>(), httpClientFactory.Object);

        var web3 = provider.Web3(97);

        Assert.NotNull(web3);
        Assert.Equal("http://rpc/evm/97", createdForUrl);
    }

    private static IHttpClientFactory MockHttpClientFactory()
    {
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<Action<HttpRequestHeaders>>())).Returns(new HttpClient());
        return httpClientFactory.Object;
    }
}
