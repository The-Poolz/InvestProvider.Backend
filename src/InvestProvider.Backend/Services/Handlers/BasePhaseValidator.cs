using FluentValidation;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Handlers.Contexts;

namespace InvestProvider.Backend.Services.Handlers;

public abstract class BasePhaseValidator<T> : AbstractValidator<T>
    where T : IPhaseRequest
{
    protected static bool HasCurrentPhase(IPhaseRequest model) =>
        model.Context.StrapiProjectInfo?.CurrentPhase != null;

    protected static bool HasProjectsInformation(IPhaseRequest model) =>
        model.Context.DynamoDbProjectsInfo != null;

    protected static bool SetPhase(IPhaseRequest model)
    {
        var phase = model.Context.StrapiProjectInfo?.Phases.FirstOrDefault(p => p.Id == model.Context.PhaseId);
        model.Context.Phase = phase!;
        return phase != null;
    }

    protected static bool HasWhiteList(IPhaseRequest model) =>
        model.Context.WhiteList != null;

    protected static IRuleBuilderOptions<TModel, TModel> WhiteListPhaseRules<TModel>(BasePhaseValidator<TModel> validator)
        where TModel : IPhaseRequest
    {
        return validator.RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(m => HasProjectsInformation(m))
            .WithError(Error.POOLZ_BACK_ID_NOT_FOUND, x => new { x.ProjectId })
            .Must(m => HasCurrentPhase(m))
            .WithError(Error.NOT_FOUND_ACTIVE_PHASE, x => new { x.ProjectId })
            .Must(m => SetPhase(m))
            .WithError(Error.PHASE_IN_PROJECT_NOT_FOUND, x => new { x.ProjectId, x.PhaseId })
            .Must(x => DateTime.UtcNow < x.Context.Phase!.Finish)
            .WithError(Error.PHASE_FINISHED, x => new { EndTime = x.Context.Phase!.Finish, NowTime = DateTime.UtcNow })
            .Must(x => x.Context.Phase!.MaxInvest == 0)
            .WithError(Error.PHASE_IS_NOT_WHITELIST)
            .When(x => x.Context.StrapiProjectInfo!.CurrentPhase!.MaxInvest == 0, ApplyConditionTo.CurrentValidator);
    }
}
