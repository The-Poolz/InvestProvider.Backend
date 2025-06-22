using InvestProvider.Backend.Services.DynamoDb.Models;

namespace InvestProvider.Backend.Services.Validators.Models;

public interface IValidatedDynamoDbProjectInfo : IProjectContext
{
    ProjectsInformation DynamoDbProjectsInfo { get; set; }
}
