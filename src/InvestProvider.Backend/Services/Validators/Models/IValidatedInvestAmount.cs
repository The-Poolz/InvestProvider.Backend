namespace InvestProvider.Backend.Services.Validators.Models;

public interface IValidatedInvestAmount :  IExistActivePhase, IValidatedDynamoDbProjectInfo
{
    public decimal Amount { get; set; }
    public byte TokenDecimals { get; set; }
    public string WeiAmount { get; }
}
