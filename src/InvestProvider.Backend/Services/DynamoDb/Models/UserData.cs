using Amazon.DynamoDBv2.DataModel;

namespace InvestProvider.Backend.Services.DynamoDb.Models;

[DynamoDBTable("InvestProvider.Userdata")]
public class UserData
{
    [DynamoDBHashKey("PhaseId")]
    public string PhaseId { get; set; } = null!;

    [DynamoDBRangeKey("UserAddress")]
    public string UserAddress { get; set; } = null!;

    [DynamoDBProperty("Amount")]
    public decimal Amount { get; set; }
}