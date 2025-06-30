using FluentValidation;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Handlers.MyAllocation.Models;

namespace InvestProvider.Backend.Services.Handlers.MyAllocation;

public class MyAllocationValidator : BasePhaseValidator<MyAllocationRequest>
{
    public MyAllocationValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        WhiteListPhaseRules(this)
            .Must(HasWhiteList)
            .When(x => x.Context.StrapiProjectInfo!.CurrentPhase!.MaxInvest == 0, ApplyConditionTo.CurrentValidator)
            .WithError(Error.NOT_IN_WHITE_LIST, x => new { x.ProjectId, PhaseId = x.Context.StrapiProjectInfo!.CurrentPhase!.Id, UserAddress = x.UserAddress.Address });
    }
}
