using Poolz.Finance.CSharp.Strapi;

namespace InvestProvider.Backend.Services.Strapi.Models;

public class ProjectInfo
{
    public ProjectInfo(ProjectInfoResponse graphQlResponse)
    {
        ArgumentNullException.ThrowIfNull(graphQlResponse);

        var projectsInformation = graphQlResponse.ProjectsInfo
            ?? throw new InvalidOperationException("Project information is missing in Strapi response.");

        Phases = projectsInformation.ProjectPhases ?? Array.Empty<ComponentPhaseStartEndAmount>();

        ChainId = projectsInformation.ChainSetting?.Chain?.ChainId
            ?? throw new InvalidOperationException("ChainSetting/Chain is missing in Strapi response to receive project ChainId.");
    }

    public ComponentPhaseStartEndAmount? CurrentPhase => Phases.Count == 0 ? null : Phases.First();

    public ICollection<ComponentPhaseStartEndAmount> Phases { get; }

    public long ChainId { get; }
}