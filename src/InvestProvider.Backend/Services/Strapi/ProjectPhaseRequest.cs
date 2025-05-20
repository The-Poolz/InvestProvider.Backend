using GraphQL;
using Poolz.Finance.CSharp.Strapi;

namespace InvestProvider.Backend.Services.Strapi;

public static class ProjectPhaseRequest
{
    public static GraphQLRequest BuildRequest(string phaseId)
    {
        var documentIdFilter = new GraphQlQueryParameter<string>("documentId", defaultValue: phaseId);
        var statusParam = new GraphQlQueryParameter<PublicationStatus?>("status", "PublicationStatus", PublicationStatus.Published);

        var queryBuilder = new QueryQueryBuilder()
            .WithProjectPhase(new ProjectPhaseQueryBuilder()
                .WithStartTime()
                .WithEndTime()
                .WithMaxInvest()
                .WithProjectsInformation(new ProjectsInformationQueryBuilder()
                    .WithPoolzBackId()
                    .WithUploadPool(new ComponentUploadPoolUploadPoolQueryBuilder()
                        .WithBuyWith(new BuyWithQueryBuilder()
                            .WithChainAddresses(new ComponentChainAddressesChainAddressesQueryBuilder()
                                .WithAddress()
                            )
                        )
                    )
                    .WithChainSetting(new ChainSettingQueryBuilder()
                        .WithChain(new ChainQueryBuilder()
                            .WithChainId()
                        )
                    )
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