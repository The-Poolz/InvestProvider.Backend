using Xunit;
using InvestProvider.Backend.Services.Strapi;

namespace InvestProvider.Backend.Tests.Services;

public class ProjectPhaseRequestTests
{
    [Fact]
    public void BuildRequest_NoPhaseFilter_DoesNotIncludeFilterParameters()
    {
        var request = ProjectPhaseRequest.BuildRequest("pid", false);
        var query = request.Query;

        Assert.Contains("$documentId:ID=\"pid\"", query);
        Assert.Contains("$status:PublicationStatus=PUBLISHED", query);
        Assert.DoesNotContain("$phaseFilter", query);
        Assert.DoesNotContain("$sortFilter", query);
        Assert.DoesNotContain("filters:$phaseFilter", query);
        Assert.DoesNotContain("sort:$sortFilter", query);
    }

    [Fact]
    public void BuildRequest_WithPhaseFilter_IncludesFilterParameters()
    {
        var request = ProjectPhaseRequest.BuildRequest("pid", true);
        var query = request.Query;

        Assert.Contains("$documentId:ID=\"pid\"", query);
        Assert.Contains("$status:PublicationStatus=PUBLISHED", query);
        Assert.Contains("$phaseFilter:ComponentPhaseStartEndAmountFiltersInput", query);
        Assert.Contains("$sortFilter:[String]", query);
        Assert.Contains("filters:$phaseFilter", query);
        Assert.Contains("sort:$sortFilter", query);
        Assert.Contains("Start:{lte", query);
        Assert.Contains("Finish:{gte", query);
    }
}
