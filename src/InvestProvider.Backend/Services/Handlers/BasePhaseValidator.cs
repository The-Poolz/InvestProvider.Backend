using FluentValidation;
using Net.Utils.ErrorHandler.Extensions;
using Amazon.DynamoDBv2.DataModel;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Validators.Models;

namespace InvestProvider.Backend.Services.Handlers;

public abstract class BasePhaseValidator<T>(IStrapiClient strapi, IDynamoDBContext dynamoDb) : AbstractValidator<T>
    where T : IExistActivePhase
{
    protected readonly IStrapiClient _strapi = strapi;
    protected readonly IDynamoDBContext _dynamoDb = dynamoDb;

    protected bool NotNullCurrentPhase(IExistActivePhase model)
    {
        model.StrapiProjectInfo = _strapi.ReceiveProjectInfoAsync(model.ProjectId, filterPhases: model.FilterPhases)
            .GetAwaiter()
            .GetResult();
        return model.StrapiProjectInfo.CurrentPhase != null;
    }

    protected async Task<bool> NotNullProjectsInformationAsync<TModel>(TModel model, CancellationToken token)
        where TModel : IValidatedDynamoDbProjectInfo
    {
        model.DynamoDbProjectsInfo = await _dynamoDb.LoadAsync<ProjectsInformation>(model.ProjectId, token);
        return model.DynamoDbProjectsInfo != null;
    }

    protected static bool SetPhase(IExistPhase model)
    {
        var phase = model.StrapiProjectInfo.Phases.FirstOrDefault(p => p.Id == model.PhaseId);
        model.Phase = phase!;
        return phase != null;
    }

    protected async Task<bool> NotNullWhiteListAsync<TModel>(TModel model, CancellationToken token)
        where TModel : IWhiteListUser
    {
        model.WhiteList = await _dynamoDb.LoadAsync<WhiteList>(
            hashKey: WhiteList.CalculateHashId(model.ProjectId, model.StrapiProjectInfo.CurrentPhase!.Start!.Value),
            rangeKey: model.UserAddress.Address,
            token
        );
        return model.WhiteList != null;
    }

    protected static IRuleBuilderOptions<TModel, TModel> WhiteListPhaseRules<TModel>(BasePhaseValidator<TModel> validator)
        where TModel : IExistPhase, IValidatedDynamoDbProjectInfo
    {
        return validator.RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .MustAsync((m, ct) => validator.NotNullProjectsInformationAsync(m, ct))
            .WithError(Error.POOLZ_BACK_ID_NOT_FOUND, x => new { x.ProjectId })
            .Must(m => validator.NotNullCurrentPhase(m))
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
