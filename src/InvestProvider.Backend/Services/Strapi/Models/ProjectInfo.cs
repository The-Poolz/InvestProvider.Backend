using Poolz.Finance.CSharp.Strapi;

namespace InvestProvider.Backend.Services.Strapi.Models;

public class ProjectInfo(ProjectInfoResponse graphQlResponse)
{
    public ComponentPhaseStartEndAmount? CurrentPhase => Phases.FirstOrDefault(x => x.Start <= DateTime.UtcNow || x.Finish > DateTime.UtcNow);
    public IEnumerable<ComponentPhaseStartEndAmount> Phases => graphQlResponse.ProjectsInfo.ProjectPhases;
    public int PoolId { get; } = graphQlResponse.ProjectsInfo.PoolzBackId!.Value;
    public long ChainId { get; } = graphQlResponse.ProjectsInfo.ChainSetting.Chain.ChainId!.Value;
}