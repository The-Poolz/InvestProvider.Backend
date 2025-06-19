using FluentValidation;
using Amazon.DynamoDBv2.DataModel;
using Net.Utils.ErrorHandler.Extensions;
using poolz.finance.csharp.contracts.InvestProvider;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Validators.Models;

namespace InvestProvider.Backend.Services.Validators;

public class WhiteListSignatureValidator : SignatureValidatorBase<IWhiteListSignature>
{
    public WhiteListSignatureValidator(IDynamoDBContext dynamoDb, IInvestProviderService<ContractType> investProvider)
        : base(investProvider)
    {
        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .MustAsync(async (x, cancellationToken) =>
            {
                x.WhiteList = await dynamoDb.LoadAsync<WhiteList>(
                    hashKey: WhiteList.CalculateHashId(x.ProjectId, x.StrapiProjectInfo.CurrentPhase!.Start!.Value),
                    rangeKey: x.UserAddress.Address,
                    cancellationToken
                );
                return x.WhiteList != null;
            })
            .WithError(Error.NOT_IN_WHITE_LIST)
            .Must(x => x.Amount + x.InvestedAmount <= x.WhiteList.Amount)
            .WithError(Error.AMOUNT_EXCEED_MAX_WHITE_LIST_AMOUNT);
    }
}