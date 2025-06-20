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
    private readonly IDynamoDBContext _dynamoDb;

    public WhiteListSignatureValidator(IDynamoDBContext dynamoDb, IInvestProviderService<ContractType> investProvider)
        : base(investProvider)
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
            })
            .Must(x => x.Amount + x.InvestedAmount <= x.WhiteList.Amount)
            .WithError(Error.AMOUNT_EXCEED_MAX_WHITE_LIST_AMOUNT, x => new
            {
                UserAmount = x.Amount,
                MaxInvestAmount = x.WhiteList.Amount,
                InvestSum = x.InvestedAmount
            });
    }

    private async Task<bool> NotNullWhiteListAsync(IWhiteListSignature model, CancellationToken cancellationToken)
    {
        model.WhiteList = await _dynamoDb.LoadAsync<WhiteList>(
            hashKey: WhiteList.CalculateHashId(model.ProjectId, model.StrapiProjectInfo.CurrentPhase!.Start!.Value),
            rangeKey: model.UserAddress.Address,
            cancellationToken
        );
        return model.WhiteList != null;
    }
}