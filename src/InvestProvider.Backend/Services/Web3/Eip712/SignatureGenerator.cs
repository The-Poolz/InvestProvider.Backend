using Nethereum.Signer.EIP712;
using InvestProvider.Backend.Services.Web3.Eip712.Models;

namespace InvestProvider.Backend.Services.Web3.Eip712;

public class SignatureGenerator(ISignerManager signerManager) : ISignatureGenerator
{
    public string GenerateSignature(Eip712Domain domain, InvestMessage investMessage)
    {
        var typedData = new Eip712TypedData(
            domain,
            investMessage
        );
        return new Eip712TypedDataSigner().SignTypedDataV4(
            json: typedData.ToEip712Json(),
            key: signerManager.GetSigner()
        );
    }
}