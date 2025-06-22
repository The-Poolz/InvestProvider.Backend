using InvestProvider.Backend.Services.Strapi.Models;

namespace InvestProvider.Backend.Services.Validators.Models;

public interface IExistActivePhase : IProjectContext
{
    bool FilterPhases { get; }
    ProjectInfo StrapiProjectInfo { get; set; }
}
