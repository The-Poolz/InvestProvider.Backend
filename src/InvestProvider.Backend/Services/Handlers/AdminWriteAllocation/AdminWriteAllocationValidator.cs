using FluentValidation;
using Amazon.DynamoDBv2.DataModel;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminWriteAllocation;

public class AdminWriteAllocationValidator : BasePhaseValidator<AdminWriteAllocationRequest>
{

    public AdminWriteAllocationValidator(IStrapiClient strapi, IDynamoDBContext dynamoDb)
        : base(strapi, dynamoDb)
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        WhiteListPhaseRules(this);
    }

}
