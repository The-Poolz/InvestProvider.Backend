using GraphQL;
using Poolz.Finance.CSharp.Strapi;

namespace InvestProvider.Backend.Services.Strapi;

public static class ProjectPhaseRequest
{
    public static GraphQLRequest BuildRequest(string projectId, bool filterPhases)
    {
        var documentIdFilter = new GraphQlQueryParameter<string>("documentId", defaultValue: projectId);
        var statusParam = new GraphQlQueryParameter<PublicationStatus?>("status", "PublicationStatus", PublicationStatus.Published);
        GraphQlQueryParameter<ComponentPhaseStartEndAmountFiltersInput>? phaseFilter = null;
        GraphQlQueryParameter<IEnumerable<string>>? sortFilter = null;

        if (filterPhases)
        {
            phaseFilter = new GraphQlQueryParameter<ComponentPhaseStartEndAmountFiltersInput>("phaseFilter", new ComponentPhaseStartEndAmountFiltersInput
            {
                And = new[]
                {
                    new ComponentPhaseStartEndAmountFiltersInput
                    {
                        Start = new DateTimeFilterInput { Lte = DateTime.UtcNow }
                    },
                    new ComponentPhaseStartEndAmountFiltersInput
                    {
                        Finish = new DateTimeFilterInput { Gte = DateTime.UtcNow }
                    }
                }
            });

            sortFilter = new GraphQlQueryParameter<IEnumerable<string>>("sortFilter", "[String]", ["Start"]);
        }

        var queryBuilder = new QueryQueryBuilder()
            .WithProjectsInformation(new ProjectsInformationQueryBuilder()
                .WithChainSetting(new ChainSettingQueryBuilder()
                    .WithChain(new ChainQueryBuilder()
                        .WithChainId()
                    )
                )
                .WithProjectPhases(new ComponentPhaseStartEndAmountQueryBuilder()
                    .WithId()
                    .WithStart()
                    .WithFinish()
                    .WithMaxInvest(),
                    filters: filterPhases ? phaseFilter : null,
                    sort: filterPhases ? sortFilter : null
                ),
                documentId: documentIdFilter,
                status: statusParam
            )
            .WithParameter(documentIdFilter)
            .WithParameter(statusParam);

        if (filterPhases)
        {
            queryBuilder
                .WithParameter(phaseFilter)
                .WithParameter(sortFilter);
        }

        return new GraphQLRequest
        {
            Query = queryBuilder.Build()
        };
    }
}