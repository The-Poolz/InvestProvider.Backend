using InvestProvider.Backend.Services.DynamoDb.Models;

namespace InvestProvider.Backend.Services.Validators.Models;

public interface IValidatedDynamoDbProjectInfo
{
    public string ProjectId { get; }
    public ProjectsInformation DynamoDbProjectsInfo { get; set; }
}