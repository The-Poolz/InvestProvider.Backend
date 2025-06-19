using InvestProvider.Backend.Services.DynamoDb.Models;

namespace InvestProvider.Backend.Services.Validators.Models;

public interface IValidatedDynamoDbProjectInfo : IHasProjectId
{
    public ProjectsInformation DynamoDbProjectsInfo { get; set; }
}