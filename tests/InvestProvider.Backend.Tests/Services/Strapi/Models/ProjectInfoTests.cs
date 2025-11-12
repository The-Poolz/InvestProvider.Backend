using Xunit;
using System;
using FluentAssertions;
using FluentValidation;
using System.Collections;
using System.Collections.Generic;
using Poolz.Finance.CSharp.Strapi;
using InvestProvider.Backend.Services.Strapi.Models;

namespace InvestProvider.Backend.Tests.Services.Strapi.Models;

public class ProjectInfoTests
{
    [Fact]
    public void Constructor_PopulatesChainIdAndPhases()
    {
        var phase = (ComponentPhaseStartEndAmount)TestHelpers.CreatePhase(
            "phase-1",
            new DateTime(2023, 01, 02, 00, 00, 00, DateTimeKind.Utc),
            new DateTime(2023, 01, 03, 00, 00, 00, DateTimeKind.Utc),
            10m
        );
        var chainId = 100;

        var projectInfo = TestHelpers.CreateProjectInfo(chainId, phase);

        projectInfo.ChainId.Should().Be(chainId);
        projectInfo.Phases.Should().BeEquivalentTo([phase]);
        projectInfo.CurrentPhase.Should().BeEquivalentTo(phase);
    }

    [Fact]
    public void CurrentPhase_ReturnsNull_WhenNoPhases()
    {
        var phaseType = Type.GetType("Poolz.Finance.CSharp.Strapi.ComponentPhaseStartEndAmount, Poolz.Finance.CSharp.Strapi")!;
        var phases = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(phaseType))!;

        var projectInfo = TestHelpers.CreateProjectInfo(55, phases);

        projectInfo.Phases.Should().BeEmpty();
        projectInfo.CurrentPhase.Should().BeNull();
    }

    [Fact]
    public void Constructor_Throws_WhenProjectsInfoMissing()
    {
        var projectId = "projId";

        var testCode = () => new ProjectInfo(new ProjectInfoResponse(null!), projectId);

        testCode.Should().Throw<ValidationException>()
            .Which.Errors.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new
            {
                ErrorCode = nameof(Error.STRAPI_MISSING_PROJECT_DATA),
                ErrorMessage = "Project information is missing in Strapi response.",
                CustomState = new
                {
                    ProjectId = projectId
                }
            });
    }

    [Fact]
    public void Constructor_Throws_WhenChainDataMissing()
    {
        var projectId = "projId";

        var testCode = () => new ProjectInfo(new ProjectInfoResponse(new ProjectsInformation()), projectId);

        testCode.Should().Throw<ValidationException>()
            .Which.Errors.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new
            {
                ErrorCode = nameof(Error.STRAPI_MISSING_CHAIN_DATA),
                ErrorMessage = "ChainSetting/Chain is missing in project, to receive project ChainId.",
                CustomState = new
                {
                    ProjectId = projectId
                }
            });
    }
}