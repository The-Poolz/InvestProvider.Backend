using Newtonsoft.Json;
using InvestProvider.Backend.Services.Strapi.Models;

namespace InvestProvider.Backend.Services.Validators.Models;

public interface IHasProjectInfo
{
    [JsonIgnore]
    public ProjectInfo? ProjectInfo { get; set; }
}