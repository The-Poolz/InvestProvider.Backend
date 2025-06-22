using FluentValidation;
using Amazon.DynamoDBv2.DataModel;
using InvestProvider.Backend.Services.Strapi;
using DynamoProjectsInformation = InvestProvider.Backend.Services.DynamoDb.Models.ProjectsInformation;
using InvestProvider.Backend.Services.DynamoDb.Models;
using Net.Web3.EthereumWallet;
using InvestProvider.Backend.Services.Validators;
using System.Collections.Generic;
using Poolz.Finance.CSharp.Strapi;

namespace InvestProvider.Backend.Services.Handlers;

public abstract class BasePhaseValidator<T> : AbstractValidator<T>
{
    protected readonly IStrapiClient _strapi;
    protected readonly IDynamoDBContext _dynamoDb;

    protected BasePhaseValidator(IStrapiClient strapi, IDynamoDBContext dynamoDb)
    {
        _strapi = strapi;
        _dynamoDb = dynamoDb;
    }

    protected bool NotNullCurrentPhase(T model)
    {
        dynamic m = model;
        m.PhaseContext.StrapiProjectInfo = _strapi.ReceiveProjectInfo(m.ProjectId, filterPhases: m.FilterPhases);
        return m.PhaseContext.StrapiProjectInfo.CurrentPhase != null;
    }

    protected async Task<bool> NotNullProjectsInformationAsync(T model, CancellationToken token)
    {
        dynamic m = model;
        m.PhaseContext.DynamoDbProjectsInfo = await _dynamoDb.LoadAsync<DynamoProjectsInformation>(m.ProjectId, token);
        return m.PhaseContext.DynamoDbProjectsInfo != null;
    }

    protected static bool SetPhase(T model)
    {
        dynamic m = model;
        var phase = ((IEnumerable<ComponentPhaseStartEndAmount>)m.PhaseContext.StrapiProjectInfo.Phases).FirstOrDefault(p => p.Id == m.PhaseId);
        m.PhaseContext.Phase = phase!;
        return phase != null;
    }

    protected async Task<bool> NotNullWhiteListAsync(T model, CancellationToken token)
    {
        dynamic m = model;
        m.PhaseContext.WhiteList = await _dynamoDb.LoadAsync<WhiteList>(
            WhiteList.CalculateHashId(m.ProjectId, m.PhaseContext.StrapiProjectInfo.CurrentPhase!.Start!),
            ((EthereumAddress)m.UserAddress).Address,
            token
        );
        return m.PhaseContext.WhiteList != null;
    }
}
