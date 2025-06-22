using Poolz.Finance.CSharp.Strapi;

namespace InvestProvider.Backend.Services.Validators.Models;

public interface IExistPhase : IExistActivePhase
{
    ComponentPhaseStartEndAmount Phase { get; set; }
}
