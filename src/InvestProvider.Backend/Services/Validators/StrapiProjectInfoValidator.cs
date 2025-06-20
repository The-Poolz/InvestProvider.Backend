using FluentValidation;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Validators.Models;

namespace InvestProvider.Backend.Services.Validators;

public class StrapiProjectInfoValidator : AbstractValidator<IValidatedStrapiProjectInfo>
{
    private readonly IStrapiClient _strapi;

    public StrapiProjectInfoValidator(IStrapiClient strapi)
    {
        _strapi = strapi;

        RuleFor(x => x)
            .Must(NotNullProjectsInformation)
            .WithError(Error.NOT_FOUND_ACTIVE_PHASE, x => new
            {
                x.ProjectId
            });
    }

    private bool NotNullProjectsInformation(IValidatedStrapiProjectInfo model)
    {
        model.StrapiProjectInfo = _strapi.ReceiveProjectInfo(model.ProjectId, filterPhases: model.FilterPhases);
        return model.StrapiProjectInfo.CurrentPhase != null;
    }
}