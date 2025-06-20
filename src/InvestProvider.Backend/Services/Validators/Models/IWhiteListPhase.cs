using Poolz.Finance.CSharp.Strapi;

namespace InvestProvider.Backend.Services.Validators.Models;

public interface IWhiteListPhase
{
    public ComponentPhaseStartEndAmount Phase { get; }
}