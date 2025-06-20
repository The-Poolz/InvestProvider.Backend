using FluentValidation;
using InvestProvider.Backend.Services.Validators.Models;
using InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminWriteAllocation;

public class AdminWriteAllocationValidator : AbstractValidator<AdminWriteAllocationRequest>
{
    public AdminWriteAllocationValidator(
        IValidator<IExistActivePhase> existActivePhaseValidator,
        IValidator<IValidatedDynamoDbProjectInfo> dynamoDbProjectInfoValidator,
        IValidator<IExistPhase> existPhaseValidator,
        IValidator<IWhiteListPhase> whiteListValidator
    )
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x)
            .SetValidator(dynamoDbProjectInfoValidator);

        RuleFor(x => x)
            .SetValidator(existActivePhaseValidator);

        RuleFor(x => x)
            .SetValidator(existPhaseValidator);

        RuleFor(x => x)
            .SetValidator(whiteListValidator);
    }
}