using System;
using System.Collections;
using System.Collections.Generic;
using InvestProvider.Backend.Services.Strapi.Models;

namespace InvestProvider.Backend.Tests;

public static class TestHelpers
{
    public static object CreatePhase(string id, DateTime start, DateTime finish, decimal maxInvest)
    {
        var type = Type.GetType("Poolz.Finance.CSharp.Strapi.ComponentPhaseStartEndAmount, Poolz.Finance.CSharp.Strapi")!;
        var obj = Activator.CreateInstance(type)!;
        type.GetProperty("Id")?.SetValue(obj, id);
        type.GetProperty("Start")?.SetValue(obj, (DateTime?)start);
        type.GetProperty("Finish")?.SetValue(obj, (DateTime?)finish);
        var maxInvestProp = type.GetProperty("MaxInvest");
        if (maxInvestProp != null)
        {
            var targetType = Nullable.GetUnderlyingType(maxInvestProp.PropertyType) ?? maxInvestProp.PropertyType;
            object? converted = Convert.ChangeType(maxInvest, targetType);
            maxInvestProp.SetValue(obj, converted);
        }
        return obj;
    }

    public static ProjectInfo CreateProjectInfo(long chainId, object phase)
    {
        var projectsInfoType = Type.GetType("Poolz.Finance.CSharp.Strapi.ProjectsInformation, Poolz.Finance.CSharp.Strapi")!;
        var chainSettingType = Type.GetType("Poolz.Finance.CSharp.Strapi.ChainSetting, Poolz.Finance.CSharp.Strapi")!;
        var chainType = Type.GetType("Poolz.Finance.CSharp.Strapi.Chain, Poolz.Finance.CSharp.Strapi")!;

        var phasesList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(phase.GetType()))!;
        phasesList.Add(phase);

        var chain = Activator.CreateInstance(chainType)!;
        chainType.GetProperty("ChainId")?.SetValue(chain, (long?)chainId);

        var chainSetting = Activator.CreateInstance(chainSettingType)!;
        chainSettingType.GetProperty("Chain")?.SetValue(chainSetting, chain);

        var projectsInfo = Activator.CreateInstance(projectsInfoType)!;
        projectsInfoType.GetProperty("ChainSetting")?.SetValue(projectsInfo, chainSetting);
        projectsInfoType.GetProperty("ProjectPhases")?.SetValue(projectsInfo, phasesList);

        var projectInfoResponse = new ProjectInfoResponse((dynamic)projectsInfo);
        return new ProjectInfo(projectInfoResponse);
    }
}
