namespace InvestProvider.Backend.Services.Validators.Models;

public interface IValidatedInvestAmount : IHasWeiAmount, IValidatedStrapiProjectInfo, IValidatedDynamoDbProjectInfo
{
    public decimal Amount { get; set; }
    public byte TokenDecimals { get; set; }
}