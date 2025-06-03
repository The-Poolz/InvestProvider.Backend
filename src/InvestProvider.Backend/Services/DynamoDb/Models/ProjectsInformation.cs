using Amazon.DynamoDBv2.DataModel;

namespace InvestProvider.Backend.Services.DynamoDb.Models;

[DynamoDBTable("InvestProvider.Userdata")]
public class ProjectsInformation
{
    [DynamoDBHashKey("ProjectId")]
    public string ProjectId { get; set; } = null!;

    [DynamoDBProperty("PoolzBackId")]
    public long PoolzBackId { get; set; }
}