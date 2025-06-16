using FluentValidation;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Validators.Models;

namespace InvestProvider.Backend.Services.Validators;

public class FcfsSignatureValidator : AbstractValidator<IFcfsSignature>
{
    public FcfsSignatureValidator()
    {
        RuleFor(x => x)
            .Must(x => x.Amount <= x.StrapiProjectInfo.CurrentPhase!.MaxInvest)
            .WithError(Error.AMOUNT_EXCEED_MAX_INVEST)
            .Must(x => x.InvestedAmount > 0)
            .WithError(Error.ALREADY_INVESTED);
    }
}