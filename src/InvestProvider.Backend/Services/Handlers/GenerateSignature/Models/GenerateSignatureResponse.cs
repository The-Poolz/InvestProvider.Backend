using Newtonsoft.Json;

namespace InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

public class GenerateSignatureResponse(string signature, DateTime validUntil)
{
    [JsonRequired]
    public string Signature { get; set; } = signature;

    [JsonRequired]
    public DateTime ValidUntil { get; set; } = validUntil;
}