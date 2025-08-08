using Newtonsoft.Json;
using Amazon.DynamoDBv2.DataModel;

namespace InvestProvider.Backend.Services.DynamoDb.Models;

[DynamoDBTable("Strapi.ProjectsInformation")]
public class ProjectsInformation
{
    [JsonRequired]
    [DynamoDBHashKey("ProjectId")]
    public string ProjectId { get; set; } = null!;

    [JsonRequired]
    [DynamoDBProperty("PoolzBackId")]
    public long PoolzBackId { get; set; }

    [DynamoDBProperty("TokenHashKey")]
    public string TokenHashKey { get; set; } = null!;
}