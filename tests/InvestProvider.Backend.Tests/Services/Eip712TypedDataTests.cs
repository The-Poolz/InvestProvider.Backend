using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xunit;
using Net.Web3.EthereumWallet;
using InvestProvider.Backend.Services.Web3.Eip712.Models;

namespace InvestProvider.Backend.Tests.Services;

public class Eip712TypedDataTests
{
    private static readonly string[] expected = ["types", "domain", "primaryType", "message"];

    [Fact]
    public void ToEip712Json_ReturnsOrderedFieldsAndValues()
    {
        var domain = new Eip712Domain(5, new EthereumAddress("0x00000000000000000000000000000000000000aa"));
        var message = new InvestMessage(
            poolId: 1,
            userAddress: new EthereumAddress("0x00000000000000000000000000000000000000bb"),
            amount: new System.Numerics.BigInteger(10),
            validUntil: DateTime.UnixEpoch,
            nonce: new System.Numerics.BigInteger(3));

        var typedData = new Eip712TypedData(domain, message);
        var json = typedData.ToEip712Json();

        var obj = JObject.Parse(json);

        Assert.Equal(expected, obj.Properties().Select(p => p.Name).ToArray());
        Assert.Equal("InvestMessage", (string)obj["primaryType"]!);
        Assert.Equal(5, (long)obj["domain"]!["chainId"]!);
        Assert.Equal("0x00000000000000000000000000000000000000aa", (string)obj["domain"]!["verifyingContract"]!);
        Assert.Equal(1, (int)obj["message"]!["poolId"]!);
        Assert.Equal("0x00000000000000000000000000000000000000bb", (string)obj["message"]!["user"]!);
        Assert.Equal(10, (int)obj["message"]!["amount"]!);
        Assert.Equal(0, (int)obj["message"]!["validUntil"]!);
        Assert.Equal(3, (int)obj["message"]!["nonce"]!);
    }
}
