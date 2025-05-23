using Newtonsoft.Json;
using Poolz.Finance.CSharp.Strapi;

namespace InvestProvider.Backend.Services.Strapi.Models;

[method: JsonConstructor]
public record ProjectInfoResponse(
    [property: JsonProperty("projectsInformation")]
    ProjectsInformation ProjectsInfo
);