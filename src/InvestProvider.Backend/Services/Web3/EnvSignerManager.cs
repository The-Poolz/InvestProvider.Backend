using Nethereum.Signer;
using EnvironmentManager.Extensions;

namespace InvestProvider.Backend.Services.Web3;

public class EnvSignerManager : ISignerManager
{
    public EthECKey GetSigner()
    {
        return new EthECKey(Env.PRIVATE_KEY_OF_LOCAL_SIGN_ACCOUNT.GetRequired<string>());
    }
}