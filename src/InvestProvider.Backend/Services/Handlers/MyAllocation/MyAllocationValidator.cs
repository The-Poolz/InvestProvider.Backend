using FluentValidation;
using Net.Utils.ErrorHandler.Extensions;
using Amazon.DynamoDBv2.DataModel;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Handlers.MyAllocation.Models;

namespace InvestProvider.Backend.Services.Handlers.MyAllocation;

public class MyAllocationValidator : BasePhaseValidator<MyAllocationRequest>
{
    public MyAllocationValidator(IStrapiClient strapi, IDynamoDBContext dynamoDb)
        : base(strapi, dynamoDb)
    {

        ClassLevelCascadeMode = CascadeMode.Stop;

        WhiteListPhaseRules(this)
            .MustAsync(NotNullWhiteListAsync)
            .WithError(Error.NOT_IN_WHITE_LIST, x => new { x.ProjectId, PhaseId = x.StrapiProjectInfo.CurrentPhase!.Id, UserAddress = x.UserAddress.Address });
    }

}
