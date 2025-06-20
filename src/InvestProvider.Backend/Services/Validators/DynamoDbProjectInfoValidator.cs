using FluentValidation;
using Amazon.DynamoDBv2.DataModel;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Validators.Models;

namespace InvestProvider.Backend.Services.Validators;

public class DynamoDbProjectInfoValidator : AbstractValidator<IValidatedDynamoDbProjectInfo>
{
    public DynamoDbProjectInfoValidator(IDynamoDBContext dynamoDb)
    {
        RuleFor(x => x)
            .MustAsync(async (x, cancellationToken) =>
            {
                x.DynamoDbProjectsInfo = await dynamoDb.LoadAsync<ProjectsInformation>(x.ProjectId, cancellationToken);
                return x.DynamoDbProjectsInfo != null;
            })
            .WithError(Error.POOLZ_BACK_ID_NOT_FOUND, x => new
            {
                x.ProjectId
            });
    }
}