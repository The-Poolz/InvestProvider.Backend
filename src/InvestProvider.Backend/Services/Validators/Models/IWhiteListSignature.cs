using InvestProvider.Backend.Services.DynamoDb.Models;

namespace InvestProvider.Backend.Services.Validators.Models;

public interface IWhiteListSignature : IHasUserInvestments
{
    public WhiteList WhiteList { get; set; }
}
