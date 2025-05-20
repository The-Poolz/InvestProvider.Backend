using Net.Web3.EthereumWallet;
using InvestProvider.Backend.Services.DynamoDb.Models;

namespace InvestProvider.Backend.Services.DynamoDb;

public class UserDataService : IUserDataService
{
    public UserData GetUserData(string phaseId, EthereumAddress userAddress)
    {
        throw new NotImplementedException();
    }
}