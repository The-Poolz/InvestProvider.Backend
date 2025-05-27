using Nethereum.Signer;

namespace InvestProvider.Backend.Services.Web3;

public interface ISignerManager
{
    public EthECKey GetSigner();
}