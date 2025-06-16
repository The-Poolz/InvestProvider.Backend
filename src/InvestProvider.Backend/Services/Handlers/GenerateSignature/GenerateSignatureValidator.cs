using FluentValidation;
using InvestProvider.Backend.Services.Validators.Models;
using InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

namespace InvestProvider.Backend.Services.Handlers.GenerateSignature;

public class GenerateSignatureRequestValidator : AbstractValidator<GenerateSignatureRequest>
{
    public GenerateSignatureRequestValidator(
        IValidator<IValidatedStrapiProjectInfo> strapiProjectInfoValidator,
        IValidator<IValidatedDynamoDbProjectInfo> dynamoDbProjectInfoValidator,
        IValidator<IValidatedInvestAmount> investAmountValidator,
        IValidator<INotAlreadyInvestedAmount> alreadyInvestedValidator,
        IValidator<IFcfsSignature> fcfsSignatureValidator,
        IValidator<IWhiteListSignature> whiteListValidator
    )
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.ProjectId).NotNull().NotEmpty();

        RuleFor(x => x.WeiAmount).NotNull().NotEmpty();

        RuleFor(x => x)
            .SetValidator(strapiProjectInfoValidator);

        RuleFor(x => x)
            .SetValidator(dynamoDbProjectInfoValidator);

        RuleFor(x => x)
            .SetValidator(investAmountValidator);

        RuleFor(x => x)
            .SetValidator(alreadyInvestedValidator);

        RuleFor(x => x)
            .SetValidator(fcfsSignatureValidator)
            .When(x => x.StrapiProjectInfo.CurrentPhase!.MaxInvest != 0);

        RuleFor(x => x)
            .SetValidator(whiteListValidator)
            .When(x => x.StrapiProjectInfo.CurrentPhase!.MaxInvest == 0);
    }
}