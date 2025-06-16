using FluentValidation;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Validators.Models;

namespace InvestProvider.Backend.Services.Validators;

public class StrapiProjectInfoValidator : AbstractValidator<IValidatedStrapiProjectInfo>
{
    public StrapiProjectInfoValidator(IStrapiClient strapi)
    {
        RuleFor(x => x)
            .Must(x =>
            {
                x.StrapiProjectInfo = strapi.ReceiveProjectInfo(x.ProjectId, filterPhases: true);
                return x.StrapiProjectInfo.CurrentPhase != null;
            })
            .WithError(Error.NOT_FOUND_ACTIVE_PHASE);
    }
}