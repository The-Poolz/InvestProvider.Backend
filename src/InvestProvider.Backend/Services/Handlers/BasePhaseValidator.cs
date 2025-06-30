using FluentValidation;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Handlers.Contexts;

namespace InvestProvider.Backend.Services.Handlers;

public abstract class BasePhaseValidator<T> : AbstractValidator<T>
    where T : IExistActivePhase
{
    protected static bool HasCurrentPhase(IExistActivePhase model) =>
        model.StrapiProjectInfo.CurrentPhase != null;

    protected static bool HasProjectsInformation(IValidatedDynamoDbProjectInfo model) =>
        model.DynamoDbProjectsInfo != null;

    protected static bool SetPhase(IExistPhase model)
    {
        var phase = model.StrapiProjectInfo.Phases.FirstOrDefault(p => p.Id == model.PhaseId);
        model.Phase = phase!;
        return phase != null;
    }

    protected static bool HasWhiteList(IWhiteListUser model) =>
        model.WhiteList != null;

    protected static IRuleBuilderOptions<TModel, TModel> WhiteListPhaseRules<TModel>(BasePhaseValidator<TModel> validator)
        where TModel : IExistPhase, IValidatedDynamoDbProjectInfo
    {
        return validator.RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(m => HasProjectsInformation(m))
            .WithError(Error.POOLZ_BACK_ID_NOT_FOUND, x => new { x.ProjectId })
            .Must(m => HasCurrentPhase(m))
            .WithError(Error.NOT_FOUND_ACTIVE_PHASE, x => new { x.ProjectId })
            .Must(m => SetPhase(m))
            .WithError(Error.PHASE_IN_PROJECT_NOT_FOUND, x => new { x.ProjectId, x.PhaseId })
            .Must(x => DateTime.UtcNow < x.Phase.Finish)
            .WithError(Error.PHASE_FINISHED, x => new { EndTime = x.Phase.Finish, NowTime = DateTime.UtcNow })
            .Must(x => x.Phase.MaxInvest == 0)
            .WithError(Error.PHASE_IS_NOT_WHITELIST)
            .When(x => x.StrapiProjectInfo.CurrentPhase!.MaxInvest == 0, ApplyConditionTo.CurrentValidator);
    }
}
