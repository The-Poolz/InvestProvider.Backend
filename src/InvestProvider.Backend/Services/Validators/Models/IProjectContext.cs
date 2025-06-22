namespace InvestProvider.Backend.Services.Validators.Models;

public interface IProjectContext
{
    string ProjectId { get; }
    string PhaseId { get; }
    long ChainId { get; }
}
