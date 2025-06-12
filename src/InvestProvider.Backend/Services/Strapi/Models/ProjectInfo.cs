using Poolz.Finance.CSharp.Strapi;

namespace InvestProvider.Backend.Services.Strapi.Models;

public class ProjectInfo(ProjectInfoResponse graphQlResponse)
{
    public ComponentPhaseStartEndAmount? CurrentPhase => Phases.Count == 0 ? null : Phases.First();
    public ICollection<ComponentPhaseStartEndAmount> Phases => graphQlResponse.ProjectsInfo.ProjectPhases;
    public long ChainId { get; } = graphQlResponse.ProjectsInfo.ChainSetting.Chain.ChainId!.Value;
}