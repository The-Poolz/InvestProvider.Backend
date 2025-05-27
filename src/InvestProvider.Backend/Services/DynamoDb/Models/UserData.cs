using Amazon.DynamoDBv2.DataModel;

namespace InvestProvider.Backend.Services.DynamoDb.Models;

[DynamoDBTable("InvestProvider.Userdata")]
public class UserData
{
    [DynamoDBHashKey]
    public string PhaseId { get; set; } = null!;

    public string UserAddress { get; set; } = null!;

    public decimal Amount { get; set; }
}