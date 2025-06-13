using FluentValidation;
using Amazon.DynamoDBv2.DataModel;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

namespace InvestProvider.Backend.Services.Handlers.GenerateSignature;

public class GenerateSignatureRequestValidator : AbstractValidator<GenerateSignatureRequest>
{
    public GenerateSignatureRequestValidator(IStrapiClient strapi, IDynamoDBContext dynamoDb)
    {
        RuleFor(x => x.ProjectId).NotNull().NotEmpty();

        RuleFor(x => x.WeiAmount).NotNull().NotEmpty();

        RuleFor(x => x)
            .Must(request =>
            {
                request.StrapiProjectInfo = strapi.ReceiveProjectInfo(request.ProjectId, filterPhases: true);
                return request.StrapiProjectInfo.CurrentPhase != null;
            })
            .WithError(Error.NOT_FOUND_ACTIVE_PHASE);

        RuleFor(x => x)
            .MustAsync(async (request, cancellationToken) =>
            {
                request.DynamoDbProjectsInfo = await dynamoDb.LoadAsync<ProjectsInformation>(request.ProjectId, cancellationToken);

                return request.DynamoDbProjectsInfo != null;
            })
            .WithError(Error.POOLZ_BACK_ID_NOT_FOUND);
    }
}