using FluentValidation;
using Net.Utils.ErrorHandler.Extensions;
using Amazon.DynamoDBv2.DataModel;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Handlers.MyAllocation.Models;
using InvestProvider.Backend.Services.DynamoDb.Models;

namespace InvestProvider.Backend.Services.Handlers.MyAllocation;

public class MyAllocationValidator : AbstractValidator<MyAllocationRequest>
{
    private readonly IStrapiClient _strapi;
    private readonly IDynamoDBContext _dynamoDb;

    public MyAllocationValidator(IStrapiClient strapi, IDynamoDBContext dynamoDb)
    {
        _strapi = strapi;
        _dynamoDb = dynamoDb;

        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .MustAsync(NotNullProjectsInformationAsync)
            .WithError(Error.POOLZ_BACK_ID_NOT_FOUND, x => new { x.ProjectId })
            .Must(NotNullCurrentPhase)
            .WithError(Error.NOT_FOUND_ACTIVE_PHASE, x => new { x.ProjectId })
            .Must(SetPhase)
            .WithError(Error.PHASE_IN_PROJECT_NOT_FOUND, x => new { x.ProjectId, x.PhaseId })
            .Must(x => DateTime.UtcNow < x.Phase.Finish)
            .WithError(Error.PHASE_FINISHED, x => new { EndTime = x.Phase.Finish, NowTime = DateTime.UtcNow })
            .Must(x => x.Phase.MaxInvest == 0)
            .WithError(Error.PHASE_IS_NOT_WHITELIST)
            .MustAsync(NotNullWhiteListAsync)
            .WithError(Error.NOT_IN_WHITE_LIST, x => new { x.ProjectId, PhaseId = x.StrapiProjectInfo.CurrentPhase!.Id, UserAddress = x.UserAddress.Address });
    }

    private bool NotNullCurrentPhase(MyAllocationRequest model)
    {
        model.StrapiProjectInfo = _strapi.ReceiveProjectInfo(model.ProjectId, filterPhases: model.FilterPhases);
        return model.StrapiProjectInfo.CurrentPhase != null;
    }

    private async Task<bool> NotNullProjectsInformationAsync(MyAllocationRequest model, CancellationToken token)
    {
        model.DynamoDbProjectsInfo = await _dynamoDb.LoadAsync<ProjectsInformation>(model.ProjectId, token);
        return model.DynamoDbProjectsInfo != null;
    }

    private bool SetPhase(MyAllocationRequest model)
    {
        var phase = model.StrapiProjectInfo.Phases.FirstOrDefault(p => p.Id == model.PhaseId);
        model.Phase = phase!;
        return phase != null;
    }

    private async Task<bool> NotNullWhiteListAsync(MyAllocationRequest model, CancellationToken token)
    {
        model.WhiteList = await _dynamoDb.LoadAsync<WhiteList>(
            hashKey: WhiteList.CalculateHashId(model.ProjectId, model.StrapiProjectInfo.CurrentPhase!.Start!.Value),
            rangeKey: model.UserAddress.Address,
            token
        );
        return model.WhiteList != null;
    }
}
