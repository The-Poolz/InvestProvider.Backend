using Poolz.Finance.CSharp.Strapi;

namespace InvestProvider.Backend.Services.Strapi.Models;

public class ProjectInfo(ProjectInfoResponse graphQlResponse)
{
    public ComponentPhaseStartEndAmount? CurrentPhase => graphQlResponse.ProjectsInfo.ProjectPhases.FirstOrDefault(x => x.Start <= DateTime.UtcNow || x.Finish > DateTime.UtcNow);
    public ICollection<ComponentPhaseStartEndAmount> Phases => graphQlResponse.ProjectsInfo.ProjectPhases;
    public long ChainId { get; } = graphQlResponse.ProjectsInfo.ChainSetting.Chain.ChainId!.Value;
}