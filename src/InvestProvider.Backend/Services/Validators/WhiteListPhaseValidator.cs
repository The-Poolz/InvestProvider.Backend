using FluentValidation;
using InvestProvider.Backend.Services.Validators.Models;
using Net.Utils.ErrorHandler.Extensions;

namespace InvestProvider.Backend.Services.Validators
{
    public class WhiteListPhaseValidator : AbstractValidator<IWhiteListPhase>
    {
        public WhiteListPhaseValidator()
        {
            RuleFor(x => x)
                .Must(x => x.Phase.MaxInvest == 0)
                .WithError(Error.PHASE_IS_NOT_WHITELIST);
        }
    }
}
