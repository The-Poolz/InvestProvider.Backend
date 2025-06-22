using FluentValidation;
    where T : IPhaseRequest
        model.PhaseContext.StrapiProjectInfo = _strapi.ReceiveProjectInfo(model.ProjectId, filterPhases: model.FilterPhases);
        return model.PhaseContext.StrapiProjectInfo.CurrentPhase != null;
        model.PhaseContext.DynamoDbProjectsInfo = await _dynamoDb.LoadAsync<DynamoProjectsInformation>(model.ProjectId, token);
        return model.PhaseContext.DynamoDbProjectsInfo != null;
        var phase = ((IEnumerable<ComponentPhaseStartEndAmount>)model.PhaseContext.StrapiProjectInfo.Phases)
            .FirstOrDefault(p => p.Id == model.PhaseId);
        model.PhaseContext.Phase = phase!;

    protected async Task<bool> NotNullWhiteListAsync(IUserPhaseRequest model, CancellationToken token)
        model.PhaseContext.WhiteList = await _dynamoDb.LoadAsync<WhiteList>(
            WhiteList.CalculateHashId(model.ProjectId, model.PhaseContext.StrapiProjectInfo.CurrentPhase!.Start!.Value),
            model.UserAddress.Address,
        return model.PhaseContext.WhiteList != null;

    protected BasePhaseValidator(IStrapiClient strapi, IDynamoDBContext dynamoDb)
    {
        _strapi = strapi;
        _dynamoDb = dynamoDb;
    }

    protected bool NotNullCurrentPhase(T model)
    {
        dynamic m = model;
        m.PhaseContext.StrapiProjectInfo = _strapi.ReceiveProjectInfo(m.ProjectId, filterPhases: m.FilterPhases);
        return m.PhaseContext.StrapiProjectInfo.CurrentPhase != null;
    }

    protected async Task<bool> NotNullProjectsInformationAsync(T model, CancellationToken token)
    {
        dynamic m = model;
        m.PhaseContext.DynamoDbProjectsInfo = await _dynamoDb.LoadAsync<DynamoProjectsInformation>(m.ProjectId, token);
        return m.PhaseContext.DynamoDbProjectsInfo != null;
    }

    protected static bool SetPhase(T model)
    {
        dynamic m = model;
        var phase = ((IEnumerable<ComponentPhaseStartEndAmount>)m.PhaseContext.StrapiProjectInfo.Phases).FirstOrDefault(p => p.Id == m.PhaseId);
        m.PhaseContext.Phase = phase!;
        return phase != null;
    }

    protected async Task<bool> NotNullWhiteListAsync(T model, CancellationToken token)
    {
        dynamic m = model;
        m.PhaseContext.WhiteList = await _dynamoDb.LoadAsync<WhiteList>(
            WhiteList.CalculateHashId(m.ProjectId, m.PhaseContext.StrapiProjectInfo.CurrentPhase!.Start!),
            ((EthereumAddress)m.UserAddress).Address,
            token
        );
        return m.PhaseContext.WhiteList != null;
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
            .WithError(Error.PHASE_IS_NOT_WHITELIST);
    }
}
