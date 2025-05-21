using Newtonsoft.Json;

namespace InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

public class GenerateSignatureResponse
{
    [JsonRequired]
    public string Signature { get; set; } = null!;

    [JsonRequired]
    public DateTime ValidUntil { get; set; }
}