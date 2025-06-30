using Amazon.DynamoDBv2.DataModel;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Validators.Models;

namespace InvestProvider.Backend.Services.Handlers.ContextBuilders;

public class PhaseContextBuilder<T>(IStrapiClient strapi, IDynamoDBContext dynamoDb)
    : IRequestContextBuilder<T>
    where T : IExistActivePhase
{
    public async Task BuildAsync(T request, CancellationToken cancellationToken)
    {
        request.StrapiProjectInfo = await strapi.ReceiveProjectInfoAsync(
            request.ProjectId,
            request.FilterPhases);

        if (request is IValidatedDynamoDbProjectInfo validated)
        {
            validated.DynamoDbProjectsInfo = await dynamoDb.LoadAsync<ProjectsInformation>(
                request.ProjectId,
                cancellationToken);
        }

        if (request is IWhiteListUser whiteListUser && request.StrapiProjectInfo.CurrentPhase != null)
        {
            whiteListUser.WhiteList = await dynamoDb.LoadAsync<WhiteList>(
                WhiteList.CalculateHashId(request.ProjectId, request.StrapiProjectInfo.CurrentPhase.Start!.Value),
                whiteListUser.UserAddress.Address,
                cancellationToken);
        }
    }
}
