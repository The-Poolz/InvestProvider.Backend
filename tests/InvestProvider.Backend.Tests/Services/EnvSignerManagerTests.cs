using System;
using Xunit;
using InvestProvider.Backend.Services.Web3;

namespace InvestProvider.Backend.Tests.Services;

public class EnvSignerManagerTests
{
    [Fact]
    public void GetSigner_ReturnsKey_FromEnvironment()
    {
        var key = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
        Environment.SetEnvironmentVariable("PRIVATE_KEY_OF_LOCAL_SIGN_ACCOUNT", key);

        var manager = new EnvSignerManager();
        var signer = manager.GetSigner();

        Assert.Equal("0x" + key, signer.GetPrivateKey());
    }
}
