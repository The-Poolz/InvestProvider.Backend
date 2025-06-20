using FluentValidation;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Validators.Models;

namespace InvestProvider.Backend.Services.Validators;

public class PhaseValidator : AbstractValidator<IValidatedPhase>
{
    public PhaseValidator()
    {
        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(x =>
            {
                var phase = x.StrapiProjectInfo.Phases.FirstOrDefault(phase => phase.Id == x.PhaseId);
                x.Phase = phase!;
                return phase != null;
            })
            .WithError(Error.PHASE_IN_PROJECT_NOT_FOUND, x => new
            {
                x.ProjectId,
                x.PhaseId
            })
            .Must(x => x.Phase.MaxInvest == 0)
            .WithError(Error.PHASE_IS_NOT_WHITELIST)
            .Must(x => DateTime.UtcNow < x.Phase.Finish)
            .WithError(Error.PHASE_FINISHED, x => new
            {
                EndTime = x.Phase.Finish,
                NowTime = DateTime.UtcNow
            });
    }
}