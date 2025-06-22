using FluentValidation;
using Net.Utils.ErrorHandler.Extensions;
using Amazon.DynamoDBv2.DataModel;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;
using InvestProvider.Backend.Services.DynamoDb.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminWriteAllocation;

public class AdminWriteAllocationValidator : AbstractValidator<AdminWriteAllocationRequest>
{
    private readonly IStrapiClient _strapi;
    private readonly IDynamoDBContext _dynamoDb;

    public AdminWriteAllocationValidator(IStrapiClient strapi, IDynamoDBContext dynamoDb)
    {
        _strapi = strapi;
        _dynamoDb = dynamoDb;

        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x)
            .MustAsync(NotNullProjectsInformationAsync)
            .WithError(Error.POOLZ_BACK_ID_NOT_FOUND, x => new { x.ProjectId });

        RuleFor(x => x)
            .Must(NotNullCurrentPhase)
            .WithError(Error.NOT_FOUND_ACTIVE_PHASE, x => new { x.ProjectId });

        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(SetPhase)
            .WithError(Error.PHASE_IN_PROJECT_NOT_FOUND, x => new { x.ProjectId, x.PhaseId })
            .Must(x => DateTime.UtcNow < x.Phase.Finish)
            .WithError(Error.PHASE_FINISHED, x => new { EndTime = x.Phase.Finish, NowTime = DateTime.UtcNow });

        RuleFor(x => x)
            .Must(x => x.Phase.MaxInvest == 0)
            .WithError(Error.PHASE_IS_NOT_WHITELIST);
    }

    private bool NotNullCurrentPhase(AdminWriteAllocationRequest model)
    {
        model.StrapiProjectInfo = _strapi.ReceiveProjectInfo(model.ProjectId, filterPhases: model.FilterPhases);
        return model.StrapiProjectInfo.CurrentPhase != null;
    }

    private async Task<bool> NotNullProjectsInformationAsync(AdminWriteAllocationRequest model, CancellationToken token)
    {
        model.DynamoDbProjectsInfo = await _dynamoDb.LoadAsync<ProjectsInformation>(model.ProjectId, token);
        return model.DynamoDbProjectsInfo != null;
    }

    private bool SetPhase(AdminWriteAllocationRequest model)
    {
        var phase = model.StrapiProjectInfo.Phases.FirstOrDefault(p => p.Id == model.PhaseId);
        model.Phase = phase!;
        return phase != null;
    }
}
