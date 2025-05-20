using Newtonsoft.Json;

namespace InvestProvider.Backend.Services.Strapi.Models;

[method: JsonConstructor]
public record ProjectPhaseResponse(
    [property: JsonProperty("projectPhase")]
    Poolz.Finance.CSharp.Strapi.ProjectPhase? ProjectPhase
);