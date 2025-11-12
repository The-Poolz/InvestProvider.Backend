using Poolz.Finance.CSharp.Strapi;
using Net.Utils.ErrorHandler.Extensions;

namespace InvestProvider.Backend.Services.Strapi.Models;

public class ProjectInfo
{
    public ProjectInfo(ProjectInfoResponse graphQlResponse, string projectId)
    {
        ArgumentNullException.ThrowIfNull(graphQlResponse);

        var projectsInformation = graphQlResponse.ProjectsInfo
            ?? throw Error.STRAPI_MISSING_PROJECT_DATA.ToException(new
            {
                ProjectId = projectId
            });

        Phases = projectsInformation.ProjectPhases ?? Array.Empty<ComponentPhaseStartEndAmount>();

        ChainId = projectsInformation.ChainSetting?.Chain?.ChainId
            ?? throw Error.STRAPI_MISSING_CHAIN_DATA.ToException(new
            {
                ProjectId = projectId
            });
    }

    public ComponentPhaseStartEndAmount? CurrentPhase => Phases.Count == 0 ? null : Phases.First();

    public ICollection<ComponentPhaseStartEndAmount> Phases { get; }

    public long ChainId { get; }
}