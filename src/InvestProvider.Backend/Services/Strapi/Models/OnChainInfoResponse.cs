using Newtonsoft.Json;
using Poolz.Finance.CSharp.Strapi;

namespace InvestProvider.Backend.Services.Strapi.Models;

[method: JsonConstructor]
public record OnChainInfoResponse([JsonProperty("chains")] ICollection<Chain> Chains);