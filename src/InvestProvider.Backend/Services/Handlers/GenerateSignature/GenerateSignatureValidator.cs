using FluentValidation;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

namespace InvestProvider.Backend.Services.Handlers.GenerateSignature;

public class GenerateSignatureRequestValidator : AbstractValidator<GenerateSignatureRequest>
{
    public GenerateSignatureRequestValidator(IStrapiClient strapi)
    {
        RuleFor(r => r.ProjectId).NotEmpty();

        RuleFor(r => r).Custom((req, ctx) =>
        {
            req.ProjectInfo = strapi.ReceiveProjectInfo(req.ProjectId, true);

            if (req.ProjectInfo.CurrentPhase is null)
            {
                ctx.AddFailure(Error.NOT_FOUND_ACTIVE_PHASE.ToErrorMessage());
            }
        });
    }
}