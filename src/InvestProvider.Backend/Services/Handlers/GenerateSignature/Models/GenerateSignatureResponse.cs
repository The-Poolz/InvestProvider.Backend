using Newtonsoft.Json;

namespace InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

public class GenerateSignatureResponse(string signature, DateTime validUntil, long poolzBackId)
{
    [JsonRequired]
    public string Signature { get; } = signature;

    [JsonRequired]
    public long ValidUntil { get; } = new DateTimeOffset(validUntil).ToUnixTimeSeconds();

    [JsonRequired]
    public long PoolzBackId { get; } = poolzBackId;
}