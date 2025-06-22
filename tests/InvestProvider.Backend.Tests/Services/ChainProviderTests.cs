using System;
using Moq;
using Xunit;
using Net.Web3.EthereumWallet;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Strapi.Models;
using InvestProvider.Backend.Services.Web3;
using InvestProvider.Backend.Services.Web3.Contracts;

namespace InvestProvider.Backend.Tests.Services;

public class ChainProviderTests
{
    [Fact]
    public void RpcUrl_IsFetched_FromStrapi_AndCached()
    {
        var info = new OnChainInfo("http://rpc", new EthereumAddress("0x00000000000000000000000000000000000000aa"), new EthereumAddress("0x00000000000000000000000000000000000000bb"));
        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveOnChainInfoAsync(1)).ReturnsAsync(info);
        var provider = new ChainProvider(strapi.Object);

        Assert.Equal("http://rpc", provider.RpcUrl(1));
        Assert.Equal("http://rpc", provider.RpcUrl(1));
        strapi.Verify(x => x.ReceiveOnChainInfoAsync(1), Times.Once);
    }

    [Fact]
    public void ContractAddress_ReturnsAddress_ForKnownType()
    {
        var info = new OnChainInfo("url", new EthereumAddress("0x00000000000000000000000000000000000000aa"), new EthereumAddress("0x00000000000000000000000000000000000000bb"));
        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveOnChainInfoAsync(3)).ReturnsAsync(info);
        var provider = new ChainProvider(strapi.Object);

        Assert.Equal("0x00000000000000000000000000000000000000aa", provider.ContractAddress(3, ContractType.InvestedProvider));
        Assert.Equal("0x00000000000000000000000000000000000000bb", provider.ContractAddress(3, ContractType.LockDealNFT));
    }

    [Fact]
    public void ContractAddress_Throws_ForUnsupportedType()
    {
        var info = new OnChainInfo("url", new EthereumAddress("0x00000000000000000000000000000000000000aa"), new EthereumAddress("0x00000000000000000000000000000000000000bb"));
        var strapi = new Mock<IStrapiClient>();
        strapi.Setup(x => x.ReceiveOnChainInfoAsync(4)).ReturnsAsync(info);
        var provider = new ChainProvider(strapi.Object);

        Assert.ThrowsAny<Exception>(() => provider.ContractAddress(4, (ContractType)99));
    }
}
