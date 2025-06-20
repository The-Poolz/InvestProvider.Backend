using FluentValidation;
using Amazon.DynamoDBv2.DataModel;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Validators.Models;

namespace InvestProvider.Backend.Services.Validators;

public class DynamoDbProjectInfoValidator : AbstractValidator<IValidatedDynamoDbProjectInfo>
{
    private readonly IDynamoDBContext _dynamoDb;

    public DynamoDbProjectInfoValidator(IDynamoDBContext dynamoDb)
    {
        _dynamoDb = dynamoDb;

        RuleFor(x => x)
            .MustAsync(NotNullProjectsInformationAsync)
            .WithError(Error.POOLZ_BACK_ID_NOT_FOUND, x => new
            {
                x.ProjectId
            });
    }

    private async Task<bool> NotNullProjectsInformationAsync(IValidatedDynamoDbProjectInfo model, CancellationToken cancellationToken)
    {
        model.DynamoDbProjectsInfo = await _dynamoDb.LoadAsync<ProjectsInformation>(model.ProjectId, cancellationToken);
        return model.DynamoDbProjectsInfo != null;
    }
}