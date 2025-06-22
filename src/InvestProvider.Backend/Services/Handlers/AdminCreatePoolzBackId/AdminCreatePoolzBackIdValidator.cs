using FluentValidation;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Strapi;
using Amazon.DynamoDBv2.DataModel;
using InvestProvider.Backend.Services.Web3.Contracts;
using poolz.finance.csharp.contracts.LockDealNFT;
using InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId.Models;
using poolz.finance.csharp.contracts.LockDealNFT.ContractDefinition;

namespace InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId;

public class AdminCreatePoolzBackIdValidator : BasePhaseValidator<AdminCreatePoolzBackIdRequest>
{
    private readonly ILockDealNFTService<ContractType> _lockDealNFT;

    public AdminCreatePoolzBackIdValidator(
        IStrapiClient strapi,
        IDynamoDBContext dynamoDb,
        ILockDealNFTService<ContractType> lockDealNFT
    ) : base(strapi, dynamoDb)
    {
        _lockDealNFT = lockDealNFT;

        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(NotNullCurrentPhase)
            .WithError(Error.NOT_FOUND_ACTIVE_PHASE, x => (new { x.ProjectId }))
            .MustAsync(CorrectProviders)
            .WithError(Error.INVALID_POOL_TYPE);
    }

    private async Task<bool> CorrectProviders(AdminCreatePoolzBackIdRequest request, CancellationToken _) =>
        (await GetFullData(request))
            .PoolInfo is [{ Name: ContractNames.InvestProvider }, { Name: ContractNames.DispenserProvider }];

    private async Task<GetFullDataOutputDTO> GetFullData(AdminCreatePoolzBackIdRequest request) =>
        await _lockDealNFT.GetFullDataQueryAsync(request.ChainId, ContractType.LockDealNFT, request.PoolzBackId);
}
