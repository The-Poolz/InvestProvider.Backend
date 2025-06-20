using InvestProvider.Backend.Services.Strapi.Models;

namespace InvestProvider.Backend.Services.Validators.Models;

public interface IExistActivePhase : IHasProjectId
{
    public bool FilterPhases { get; }
    public ProjectInfo StrapiProjectInfo { get; set; }
}