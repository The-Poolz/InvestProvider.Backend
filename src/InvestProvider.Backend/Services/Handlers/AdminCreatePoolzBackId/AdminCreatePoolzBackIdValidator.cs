using FluentValidation;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Web3.Contracts;
using poolz.finance.csharp.contracts.LockDealNFT;
using InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId;

public class AdminCreatePoolzBackIdValidator : AbstractValidator<AdminCreatePoolzBackIdRequest>
{
    private readonly IStrapiClient _strapi;
    private readonly ILockDealNFTService<ContractType> _lockDealNFT;

    public AdminCreatePoolzBackIdValidator(
        IStrapiClient strapi,
        ILockDealNFTService<ContractType> lockDealNFT
    )
    {
        _strapi = strapi;
        _lockDealNFT = lockDealNFT;

        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(NotNullCurrentPhase)
            .WithError(Error.NOT_FOUND_ACTIVE_PHASE, x => new { x.ProjectId })
            .MustAsync(async (x, _) =>
            {
                var data = await _lockDealNFT.GetFullDataQueryAsync(x.ChainId, ContractType.LockDealNFT, x.PoolzBackId);
                return data.PoolInfo is [{ Name: ContractNames.InvestProvider }, { Name: ContractNames.DispenserProvider }];
            })
            .WithError(Error.INVALID_POOL_TYPE);
    }

    private bool NotNullCurrentPhase(AdminCreatePoolzBackIdRequest model)
    {
        model.StrapiProjectInfo = _strapi.ReceiveProjectInfo(model.ProjectId, filterPhases: model.FilterPhases);
        return model.StrapiProjectInfo.CurrentPhase != null;
    }
}
