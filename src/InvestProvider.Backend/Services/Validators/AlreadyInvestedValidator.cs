using Nethereum.Util;
using FluentValidation;
using Net.Utils.ErrorHandler.Extensions;
using poolz.finance.csharp.contracts.InvestProvider;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.Validators.Models;
using InvestProvider.Backend.Services.Web3.Contracts.Models;

namespace InvestProvider.Backend.Services.Validators;

public class AlreadyInvestedValidator : AbstractValidator<INotAlreadyInvestedAmount>
{
    public AlreadyInvestedValidator(IInvestProviderService<ContractType> investProvider)
    {
        RuleFor(x => x)
            .Must(x => x.Amount <= x.StrapiProjectInfo.CurrentPhase!.MaxInvest)
            .WithError(Error.AMOUNT_EXCEED_MAX_INVEST)
            .MustAsync(async (x, _) =>
            {
                var userInvestmentsResponse = await investProvider.GetUserInvestsQueryAsync(
                    x.StrapiProjectInfo.ChainId,
                    ContractType.InvestedProvider,
                    x.DynamoDbProjectsInfo.PoolzBackId,
                    x.UserAddress
                );

                var userInvestments = userInvestmentsResponse.ReturnValue1.Select(ui => new UserInvestments(ui)).ToArray();

                var investAmounts = userInvestments
                    .Where(ui =>
                        ui.BlockCreation >= x.StrapiProjectInfo.CurrentPhase!.Start &&
                        ui.BlockCreation < x.StrapiProjectInfo.CurrentPhase.Finish
                    )
                    .Sum(ui => UnitConversion.Convert.FromWei(ui.Amount, x.TokenDecimals));

                return investAmounts == 0;
            })
            .WithError(Error.ALREADY_INVESTED)
            .When(x => x.StrapiProjectInfo.CurrentPhase!.MaxInvest != 0);
    }
}