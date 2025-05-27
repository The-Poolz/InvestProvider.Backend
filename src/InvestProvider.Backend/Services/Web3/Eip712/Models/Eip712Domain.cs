using Nethereum.ABI.EIP712;
using Net.Web3.EthereumWallet;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace InvestProvider.Backend.Services.Web3.Eip712.Models;

[Struct("EIP712Domain")]
public sealed class Eip712Domain : Domain
{
    public Eip712Domain(long chainId, EthereumAddress verifyingContract)
    {
        Name = "InvestProvider";
        Version = "1";
        ChainId = chainId;
        VerifyingContract = verifyingContract;
    }
}