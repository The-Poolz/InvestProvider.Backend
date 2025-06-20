using Nethereum.Util;
using FluentValidation;
using poolz.finance.csharp.contracts.InvestProvider;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.Validators.Models;
using InvestProvider.Backend.Services.Web3.Contracts.Models;

namespace InvestProvider.Backend.Services.Validators;

public abstract class SignatureValidatorBase<T> : AbstractValidator<T>
    where T : class, IHasUserInvestments
{
    protected SignatureValidatorBase(IInvestProviderService<ContractType> investProvider)
    {
        RuleFor(x => x).CustomAsync(async (x, _, _) =>
        {
            var response = await investProvider.GetUserInvestsQueryAsync(
                x.StrapiProjectInfo.ChainId,
                ContractType.InvestedProvider,
                x.DynamoDbProjectsInfo.PoolzBackId,
                x.UserAddress
            );

            x.UserInvestments = response.ReturnValue1
                .Select(ui => new UserInvestments(ui))
                .ToArray();

            x.InvestedAmount = x.UserInvestments
                .Where(ui => 
                    ui.BlockCreation >= x.StrapiProjectInfo.CurrentPhase!.Start && 
                    ui.BlockCreation < x.StrapiProjectInfo.CurrentPhase.Finish
                ).Sum(ui => UnitConversion.Convert.FromWei(ui.Amount, x.TokenDecimals));
        });
    }
}