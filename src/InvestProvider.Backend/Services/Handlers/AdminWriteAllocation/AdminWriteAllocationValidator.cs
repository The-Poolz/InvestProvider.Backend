using FluentValidation;
using InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminWriteAllocation;

public class AdminWriteAllocationValidator : BasePhaseValidator<AdminWriteAllocationRequest>
{

    public AdminWriteAllocationValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        WhiteListPhaseRules(this);
    }

}
