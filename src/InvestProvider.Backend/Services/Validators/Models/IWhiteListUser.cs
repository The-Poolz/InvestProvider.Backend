using InvestProvider.Backend.Services.DynamoDb.Models;

namespace InvestProvider.Backend.Services.Validators.Models;

public interface IWhiteListUser : IHasUserAddress, IExistActivePhase
{
    public WhiteList WhiteList { get; set; }
}