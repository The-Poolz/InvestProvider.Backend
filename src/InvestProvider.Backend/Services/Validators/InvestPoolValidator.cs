using FluentValidation;
using Net.Utils.ErrorHandler.Extensions;
using poolz.finance.csharp.contracts.LockDealNFT;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.Validators.Models;

namespace InvestProvider.Backend.Services.Validators;

public class InvestPoolValidator : AbstractValidator<IInvestPool>
{
    public InvestPoolValidator(ILockDealNFTService<ContractType> lockDealNFT)
    {
        RuleFor(x => x)
            .MustAsync(async (x, _) =>
            {
                var getFullData = await lockDealNFT.GetFullDataQueryAsync(x.ChainId, ContractType.LockDealNFT, x.PoolzBackId);
                return getFullData.PoolInfo is [{ Name: ContractNames.InvestProvider }, { Name: ContractNames.DispenserProvider }];
            })
            .WithError(Error.INVALID_POOL_TYPE);
    }
}