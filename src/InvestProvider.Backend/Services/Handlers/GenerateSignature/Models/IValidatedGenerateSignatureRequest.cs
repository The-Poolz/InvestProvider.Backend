using InvestProvider.Backend.Services.Strapi.Models;
using InvestProvider.Backend.Services.DynamoDb.Models;

namespace InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

public interface IValidatedGenerateSignatureRequest
{
    public ProjectInfo StrapiProjectInfo { get; set; }
    public ProjectsInformation DynamoDbProjectsInfo { get; set; }
}