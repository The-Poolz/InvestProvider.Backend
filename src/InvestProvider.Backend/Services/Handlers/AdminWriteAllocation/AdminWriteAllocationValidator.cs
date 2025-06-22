using FluentValidation;
using Net.Utils.ErrorHandler.Extensions;
using Amazon.DynamoDBv2.DataModel;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminWriteAllocation;

public class AdminWriteAllocationValidator : BasePhaseValidator<AdminWriteAllocationRequest>
{

    public AdminWriteAllocationValidator(IStrapiClient strapi, IDynamoDBContext dynamoDb)
        : base(strapi, dynamoDb)
    {

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
            .WithError(Error.PHASE_IS_NOT_WHITELIST);
    }

}
