using FluentValidation;
using Net.Utils.ErrorHandler.Extensions;
using System.Linq;
using Amazon.DynamoDBv2.DataModel;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Handlers.MyAllocation.Models;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.Web3;
using poolz.finance.csharp.contracts.InvestProvider;
using poolz.finance.csharp.contracts.InvestProvider.ContractDefinition;

namespace InvestProvider.Backend.Services.Handlers.MyAllocation;

public class MyAllocationValidator : BasePhaseValidator<MyAllocationRequest>
{
    private readonly IInvestProviderService<ContractType> _investProvider;

    public MyAllocationValidator(IStrapiClient strapi, IDynamoDBContext dynamoDb, IInvestProviderService<ContractType> investProvider)
        : base(strapi, dynamoDb)
    {
        _investProvider = investProvider;

        ClassLevelCascadeMode = CascadeMode.Stop;

        ActivePhaseRules(this)
            .CustomAsync(SetAdditionalDataAsync);

        RuleFor(x => x)
            .MustAsync(NotNullWhiteListAsync)
            .WithError(Error.NOT_IN_WHITE_LIST, x => new { x.ProjectId, PhaseId = x.StrapiProjectInfo.CurrentPhase!.Id, UserAddress = x.UserAddress.Address })
            .When(x => x.StrapiProjectInfo.CurrentPhase!.MaxInvest == 0);
    }

    private async Task SetAdditionalDataAsync(MyAllocationRequest model, ValidationContext<MyAllocationRequest> context, CancellationToken token)
    {
        if (model.StrapiProjectInfo.CurrentPhase!.MaxInvest != 0)
        {
            var response = await _investProvider.GetUserInvestsQueryAsync(
                model.StrapiProjectInfo.ChainId,
                ContractType.InvestedProvider,
                model.DynamoDbProjectsInfo.PoolzBackId,
                model.UserAddress);

            model.UsedCurrentPhase = response.ReturnValue1.Any(ui =>
            {
                var dt = DateTimeOffset.FromUnixTimeSeconds((long)ui.BlockTimestamp).UtcDateTime;
                return dt >= model.StrapiProjectInfo.CurrentPhase!.Start && dt < model.StrapiProjectInfo.CurrentPhase.Finish;
            });
        }
    }
}
