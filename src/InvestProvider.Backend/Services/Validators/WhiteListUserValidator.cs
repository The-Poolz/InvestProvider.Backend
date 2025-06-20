using FluentValidation;
using Amazon.DynamoDBv2.DataModel;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Validators.Models;

namespace InvestProvider.Backend.Services.Validators;

public class WhiteListUserValidator : AbstractValidator<IWhiteListUser>
{
    private readonly IDynamoDBContext _dynamoDb;

    public WhiteListUserValidator(IDynamoDBContext dynamoDb)
    {
        _dynamoDb = dynamoDb;

        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .MustAsync(NotNullWhiteListAsync)
            .WithError(Error.NOT_IN_WHITE_LIST, x => new
            {
                x.ProjectId,
                PhaseId = x.StrapiProjectInfo.CurrentPhase!.Id,
                UserAddress = x.UserAddress.Address
            });
    }

    private async Task<bool> NotNullWhiteListAsync(IWhiteListUser model, CancellationToken cancellationToken)
    {
        model.WhiteList = await _dynamoDb.LoadAsync<WhiteList>(
            hashKey: WhiteList.CalculateHashId(model.ProjectId, model.StrapiProjectInfo.CurrentPhase!.Start!.Value),
            rangeKey: model.UserAddress.Address,
            cancellationToken
        );
        return model.WhiteList != null;
    }
}