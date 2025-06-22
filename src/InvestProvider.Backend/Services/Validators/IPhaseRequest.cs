namespace InvestProvider.Backend.Services.Validators;

using Net.Web3.EthereumWallet;

public interface IPhaseRequest
{
    string ProjectId { get; }
    string PhaseId { get; }
    bool FilterPhases { get; }
    PhaseValidationContext PhaseContext { get; }
}

public interface IUserPhaseRequest : IPhaseRequest
{
    EthereumAddress UserAddress { get; }
}
