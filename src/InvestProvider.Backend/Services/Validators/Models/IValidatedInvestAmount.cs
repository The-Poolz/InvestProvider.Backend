using InvestProvider.Backend.Services.Strapi.Models;
using InvestProvider.Backend.Services.DynamoDb.Models;

namespace InvestProvider.Backend.Services.Validators.Models;

public interface IValidatedInvestAmount
{
    public string WeiAmount { get; }
    public decimal Amount { get; set; }
    public byte TokenDecimals { get; set; }
    public ProjectsInformation DynamoDbProjectsInfo { get; set; }
    public ProjectInfo StrapiProjectInfo { get; set; }
}