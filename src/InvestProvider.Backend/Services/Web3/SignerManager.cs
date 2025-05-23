using SecretsManager;
using Nethereum.Signer;
using EnvironmentManager.Extensions;

namespace InvestProvider.Backend.Services.Web3;

public class SignerManager(SecretManager secretManager) : ISignerManager
{
    public EthECKey GetSigner()
    {
        var privateKey = secretManager.GetSecretValue(
            secretId: Env.SECRET_ID_OF_SIGN_ACCOUNT.GetRequired<string>(),
            secretKey: Env.SECRET_KEY_OF_SIGN_ACCOUNT.GetRequired<string>()
        );
        return new EthECKey(privateKey);
    }
}