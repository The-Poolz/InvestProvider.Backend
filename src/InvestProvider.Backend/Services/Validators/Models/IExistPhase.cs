using Poolz.Finance.CSharp.Strapi;

namespace InvestProvider.Backend.Services.Validators.Models;

public interface IExistPhase : IHasPhaseId, IExistActivePhase
{
    public ComponentPhaseStartEndAmount Phase { get; set; }
}