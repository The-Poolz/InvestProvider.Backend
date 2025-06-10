using InvestProvider.Backend.Services.DynamoDb.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId.Models;

public class AdminCreatePoolzBackIdResponse : ProjectsInformation
{
    public AdminCreatePoolzBackIdResponse(ProjectsInformation projectInfo)
    {
        ProjectId = projectInfo.ProjectId;
        PoolzBackId = projectInfo.PoolzBackId;
    }
}