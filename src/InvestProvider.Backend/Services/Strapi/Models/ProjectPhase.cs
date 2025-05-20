namespace InvestProvider.Backend.Services.Strapi.Models;

public class ProjectPhase(ProjectPhaseResponse graphQlResponse)
{
    public decimal MaxInvest { get; } = graphQlResponse.ProjectPhase!.MaxInvest!.Value;
    public DateTime StartTime { get; } = graphQlResponse.ProjectPhase!.StartTime!.Value;
    public DateTime EndTime { get; } = graphQlResponse.ProjectPhase!.EndTime!.Value;
    public int PoolId { get; } = graphQlResponse.ProjectPhase!.ProjectsInformation.PoolzBackId!.Value;
    public long ChainId { get; } = graphQlResponse.ProjectPhase!.ProjectsInformation.ChainSetting.Chain.ChainId!.Value;
}