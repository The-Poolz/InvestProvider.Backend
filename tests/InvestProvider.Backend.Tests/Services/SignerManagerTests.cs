using System;
using Moq;
using SecretsManager;
using Xunit;
using InvestProvider.Backend.Services.Web3;

namespace InvestProvider.Backend.Tests.Services;

public class SignerManagerTests
{
    [Fact]
    public void GetSigner_ReturnsKey_FromSecretsManager()
    {
        var secretId = "sid";
        var secretKey = "skey";
        var privateKey = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
        Environment.SetEnvironmentVariable("SECRET_ID_OF_SIGN_ACCOUNT", secretId);
        Environment.SetEnvironmentVariable("SECRET_KEY_OF_SIGN_ACCOUNT", secretKey);

        var secrets = new Mock<SecretManager>();
        secrets.Setup(x => x.GetSecretValue(secretId, secretKey)).Returns(privateKey);

        var manager = new SignerManager(secrets.Object);
        var signer = manager.GetSigner();

        Assert.Equal("0x" + privateKey, signer.GetPrivateKey());
        secrets.Verify(x => x.GetSecretValue(secretId, secretKey), Times.Once);
    }
}
