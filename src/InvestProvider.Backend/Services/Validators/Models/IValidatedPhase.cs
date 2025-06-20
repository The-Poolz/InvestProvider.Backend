using Poolz.Finance.CSharp.Strapi;

namespace InvestProvider.Backend.Services.Validators.Models;

public interface IValidatedPhase : IHasPhaseId, IValidatedStrapiProjectInfo
{
    public ComponentPhaseStartEndAmount Phase { get; set; }
}