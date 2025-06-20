using FluentValidation;
using InvestProvider.Backend.Services.Validators.Models;
using InvestProvider.Backend.Services.Handlers.MyAllocation.Models;

namespace InvestProvider.Backend.Services.Handlers.MyAllocation;

public class MyAllocationValidator : AbstractValidator<MyAllocationRequest>
{
    public MyAllocationValidator(
        IValidator<IValidatedDynamoDbProjectInfo> dynamoDbProjectInfoValidator,
        IValidator<IExistPhase> existPhaseValidator,
        IValidator<IWhiteListPhase> whiteListValidator,
        IValidator<IWhiteListUser> whiteListUserValidator
    )
    {
        RuleFor(x => x)
            .SetValidator(dynamoDbProjectInfoValidator);

        RuleFor(x => x)
            .SetValidator(existPhaseValidator);

        RuleFor(x => x)
            .SetValidator(whiteListValidator);

        RuleFor(x => x)
            .SetValidator(whiteListUserValidator);
    }
}