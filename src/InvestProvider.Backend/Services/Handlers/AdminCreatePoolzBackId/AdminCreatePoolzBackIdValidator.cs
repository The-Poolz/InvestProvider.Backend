using FluentValidation;
using InvestProvider.Backend.Services.Validators.Models;
using InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId;

public class AdminCreatePoolzBackIdValidator : AbstractValidator<AdminCreatePoolzBackIdRequest>
{
    public AdminCreatePoolzBackIdValidator(
        IValidator<IValidatedStrapiProjectInfo> strapiProjectInfoValidator,
        IValidator<IInvestPool> investPoolValidator
    )
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x)
            .SetValidator(strapiProjectInfoValidator);

        RuleFor(x => x)
            .SetValidator(investPoolValidator);
    }
}