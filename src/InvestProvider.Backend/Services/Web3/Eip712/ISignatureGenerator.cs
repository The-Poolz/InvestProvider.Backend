using InvestProvider.Backend.Services.Web3.Eip712.Models;

namespace InvestProvider.Backend.Services.Web3.Eip712;

public interface ISignatureGenerator
{
    public string GenerateSignature(Eip712Domain domain, InvestMessage investMessage);
}