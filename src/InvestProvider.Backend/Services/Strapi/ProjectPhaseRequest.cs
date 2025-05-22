using GraphQL;
using Poolz.Finance.CSharp.Strapi;

namespace InvestProvider.Backend.Services.Strapi;

public static class ProjectPhaseRequest
{
    public static GraphQLRequest BuildRequest(string projectId)
    {
        var documentIdFilter = new GraphQlQueryParameter<string>("documentId", defaultValue: projectId);
        var statusParam = new GraphQlQueryParameter<PublicationStatus?>("status", "PublicationStatus", PublicationStatus.Published);

        var queryBuilder = new QueryQueryBuilder()
            .WithProjectsInformation(new ProjectsInformationQueryBuilder()
                .WithPoolzBackId()
                .WithChainSetting(new ChainSettingQueryBuilder()
                    .WithChain(new ChainQueryBuilder()
                        .WithChainId()
                    )
                )
                .WithProjectPhases(new ComponentPhaseStartEndAmountQueryBuilder()
                    .WithId()
                    .WithStart()
                    .WithFinish()
                    .WithMaxInvest()
                ),
                documentId: documentIdFilter,
                status: statusParam
            )
            .WithParameter(documentIdFilter)
            .WithParameter(statusParam);

        return new GraphQLRequest
        {
            Query = queryBuilder.Build()
        };
    }
}