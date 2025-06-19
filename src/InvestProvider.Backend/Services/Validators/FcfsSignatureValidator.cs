using FluentValidation;
using Net.Utils.ErrorHandler.Extensions;
using poolz.finance.csharp.contracts.InvestProvider;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.Validators.Models;

namespace InvestProvider.Backend.Services.Validators;

public class FcfsSignatureValidator : SignatureValidatorBase<IFcfsSignature>
{
    public FcfsSignatureValidator(IInvestProviderService<ContractType> investProvider)
        : base(investProvider)
    {
        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(x => x.Amount <= x.StrapiProjectInfo.CurrentPhase!.MaxInvest)
            .WithError(Error.AMOUNT_EXCEED_MAX_INVEST)
            .Must(x => x.InvestedAmount == 0)
            .WithError(Error.ALREADY_INVESTED)
            .When(x => x.StrapiProjectInfo.CurrentPhase!.MaxInvest != 0);
    }
}