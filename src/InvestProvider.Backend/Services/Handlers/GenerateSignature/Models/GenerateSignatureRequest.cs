using MediatR;
using Newtonsoft.Json;
using Net.Web3.EthereumWallet;

namespace InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

public class GenerateSignatureRequest : IRequest<GenerateSignatureResponse>
{
    [JsonRequired]
    public string PhaseId { get; set; } = null!;

    [JsonRequired]
    public EthereumAddress UserAddress { get; set; } = null!;

    [JsonRequired]
    public string WeiAmount { get; set; } = null!;
}