namespace InvestProvider.Backend.Services.Validators.Models;

public interface IValidatedInvestAmount : IHasWeiAmount, IExistActivePhase, IValidatedDynamoDbProjectInfo
{
    public decimal Amount { get; set; }
    public byte TokenDecimals { get; set; }
}