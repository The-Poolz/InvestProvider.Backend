using Amazon.DynamoDBv2.DataModel;

namespace InvestProvider.Backend.Services.DynamoDb.Models;

[DynamoDBTable("InvestProvider.Userdata")]
public class UserData
{
    [DynamoDBHashKey]
    public string PhaseId { get; set; }

    public string UserAddress { get; set; }

    public decimal Amount { get; set; }
}