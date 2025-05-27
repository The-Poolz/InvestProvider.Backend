using Nethereum.ABI.FunctionEncoding.Attributes;

namespace InvestProvider.Backend.Services.Web3.Contracts.Models;

[FunctionOutput]
public class UserInvestResponse : IFunctionOutputDTO
{
    [Parameter("tuple[]", "", order: 1)]
    public List<UserInvestRpcOutput> Invests { get; set; } = [];
}