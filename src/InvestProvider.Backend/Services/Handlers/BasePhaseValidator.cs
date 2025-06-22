using FluentValidation;
using Amazon.DynamoDBv2.DataModel;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Validators.Models;

namespace InvestProvider.Backend.Services.Handlers;

public abstract class BasePhaseValidator<T> : AbstractValidator<T>
    where T : IExistActivePhase
{
    protected readonly IStrapiClient _strapi;
    protected readonly IDynamoDBContext _dynamoDb;

    protected BasePhaseValidator(IStrapiClient strapi, IDynamoDBContext dynamoDb)
    {
        _strapi = strapi;
        _dynamoDb = dynamoDb;
    }

    protected bool NotNullCurrentPhase(IExistActivePhase model) =>
        (model.StrapiProjectInfo = _strapi.ReceiveProjectInfo(model.ProjectId, filterPhases: model.FilterPhases)).CurrentPhase != null;

    protected async Task<bool> NotNullProjectsInformationAsync<TModel>(TModel model, CancellationToken token)
        where TModel : IValidatedDynamoDbProjectInfo =>
        (model.DynamoDbProjectsInfo = await _dynamoDb.LoadAsync<ProjectsInformation>(model.ProjectId, token)) != null;

    protected bool SetPhase(IExistPhase model) =>
        (model.Phase = model.StrapiProjectInfo.Phases.FirstOrDefault(p => p.Id == model.PhaseId)) != null;

    protected async Task<bool> NotNullWhiteListAsync<TModel>(TModel model, CancellationToken token)
        where TModel : IWhiteListUser =>
        (model.WhiteList = await _dynamoDb.LoadAsync<WhiteList>(
            hashKey: WhiteList.CalculateHashId(model.ProjectId, model.StrapiProjectInfo.CurrentPhase!.Start!.Value),
            rangeKey: model.UserAddress.Address,
            token
        )) != null;
}
