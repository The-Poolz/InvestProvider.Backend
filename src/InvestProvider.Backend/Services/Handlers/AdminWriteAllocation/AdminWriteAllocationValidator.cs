using FluentValidation;
using InvestProvider.Backend.Services.Validators.Models;
using InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminWriteAllocation;

public class AdminWriteAllocationValidator : AbstractValidator<AdminWriteAllocationRequest>
{
    public AdminWriteAllocationValidator(
        IValidator<IValidatedStrapiProjectInfo> strapiProjectInfoValidator,
        IValidator<IValidatedDynamoDbProjectInfo> dynamoDbProjectInfoValidator,
        IValidator<IValidatedPhase> phaseValidator
    )
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x)
            .SetValidator(dynamoDbProjectInfoValidator);

        RuleFor(x => x)
            .SetValidator(strapiProjectInfoValidator);

        RuleFor(x => x)
            .SetValidator(phaseValidator);
    }
}