using Nethereum.Util;
using FluentValidation;
using Net.Utils.ErrorHandler.Extensions;
using poolz.finance.csharp.contracts.InvestProvider;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.Validators.Models;
using InvestProvider.Backend.Services.Web3.Contracts.Models;

namespace InvestProvider.Backend.Services.Validators;

public class FcfsSignatureValidator : AbstractValidator<IFcfsSignature>
{
    public FcfsSignatureValidator(IInvestProviderService<ContractType> investProvider)
    {
        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
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

                x.UserInvestments = userInvestmentsResponse.ReturnValue1.Select(ui => new UserInvestments(ui)).ToArray();

                x.InvestedAmount = x.UserInvestments
                    .Where(ui =>
                        ui.BlockCreation >= x.StrapiProjectInfo.CurrentPhase!.Start &&
                        ui.BlockCreation < x.StrapiProjectInfo.CurrentPhase.Finish
                    )
                    .Sum(ui => UnitConversion.Convert.FromWei(ui.Amount, x.TokenDecimals));

                return x.InvestedAmount == 0;
            })
            .WithError(Error.ALREADY_INVESTED)
            .When(x => x.StrapiProjectInfo.CurrentPhase!.MaxInvest != 0);
    }
}