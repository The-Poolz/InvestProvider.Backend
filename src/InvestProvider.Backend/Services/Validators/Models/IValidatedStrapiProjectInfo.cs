using InvestProvider.Backend.Services.Strapi.Models;

namespace InvestProvider.Backend.Services.Validators.Models;

public interface IValidatedStrapiProjectInfo
{
    public string ProjectId { get; }
    public ProjectInfo StrapiProjectInfo { get; set; }
}