using Amazon.DynamoDBv2.DataModel;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Handlers.Contexts;
using InvestProvider.Backend.Services.Handlers.AdminGetAllocation.Models;
using InvestProvider.Backend.Services.Handlers.MyUpcomingAllocation.Models;

namespace InvestProvider.Backend.Services.Handlers.ContextBuilders;

public class PhaseContextBuilder<T>(IStrapiClient strapi, IDynamoDBContext dynamoDb, PhaseContext context)
    : IRequestContextBuilder<T>
{
    public async Task BuildAsync(T request, CancellationToken cancellationToken)
    {
        if (request is not IPhaseRequest phaseRequest)
            return;

        phaseRequest.Context = context;
        context.ProjectId = phaseRequest.ProjectId;
        context.FilterPhases = phaseRequest.FilterPhases;
        context.PhaseId = phaseRequest.PhaseId;
        context.UserAddress = phaseRequest.UserAddress;
        context.WeiAmount = phaseRequest.WeiAmount;

        context.StrapiProjectInfo = await strapi.ReceiveProjectInfoAsync(
            context.ProjectId,
            context.FilterPhases);

        if (phaseRequest is not AdminGetAllocationRequest && phaseRequest is not MyUpcomingAllocationRequest)
        {
            context.DynamoDbProjectsInfo = await dynamoDb.LoadAsync<ProjectsInformation>(
                context.ProjectId,
                cancellationToken);
        }

        if (context.UserAddress != null && context.StrapiProjectInfo.CurrentPhase != null)
        {
            context.WhiteList = await dynamoDb.LoadAsync<WhiteList>(
                WhiteList.CalculateHashId(context.ProjectId, context.StrapiProjectInfo.CurrentPhase.Start!.Value),
                context.UserAddress.Address,
                cancellationToken);
        }
    }
}
