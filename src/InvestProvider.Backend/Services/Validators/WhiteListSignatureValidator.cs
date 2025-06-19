using FluentValidation;
using Amazon.DynamoDBv2.DataModel;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Validators.Models;

namespace InvestProvider.Backend.Services.Validators;

public class WhiteListSignatureValidator : AbstractValidator<IWhiteListSignature>
{
    public WhiteListSignatureValidator(IDynamoDBContext dynamoDb)
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x)
            .MustAsync(async (x, cancellationToken) =>
            {
                x.WhiteList = await dynamoDb.LoadAsync<WhiteList>(
                    hashKey: WhiteList.CalculateHashId(x.ProjectId, x.StrapiProjectInfo.CurrentPhase!.Start!.Value),
                    rangeKey: x.UserAddress.Address,
                    cancellationToken
                );
                return x.WhiteList != null;
            })
            .WithError(Error.NOT_IN_WHITE_LIST);

        RuleFor(x => x)
            .Must(x => x.Amount + x.InvestedAmount <= x.WhiteList.Amount)
            .WithError(Error.AMOUNT_EXCEED_MAX_WHITE_LIST_AMOUNT);
    }
}