using GraphQL;
using Poolz.Finance.CSharp.Strapi;

namespace InvestProvider.Backend.Services.Strapi;

public static class OnChainInfoRequest
{
    public static GraphQLRequest BuildRequest(long chainId)
    {
        var contractsFilter = new GraphQlQueryParameter<ComponentContractOnChainContractOnChainFiltersInput>("contractsFilter", new ComponentContractOnChainContractOnChainFiltersInput
        {
            Or = new[]
            {
                new ComponentContractOnChainContractOnChainFiltersInput
                {
                    ContractVersion = new ContractFiltersInput
                    {
                        NameVersion = new StringFilterInput { Contains = StrapiClient.NameOfInvestedProvider }
                    }
                },
                new ComponentContractOnChainContractOnChainFiltersInput
                {
                    ContractVersion = new ContractFiltersInput
                    {
                        NameVersion = new StringFilterInput { Contains = StrapiClient.NameOfLockDealNFT }
                    }
                }
            }

        });
        var chainFilter = new GraphQlQueryParameter<ChainFiltersInput>("chainFilter", new ChainFiltersInput
        {
            ChainId = new LongFilterInput { Eq = chainId }
        });
        var statusParam = new GraphQlQueryParameter<PublicationStatus?>("status", "PublicationStatus", PublicationStatus.Published);

        var queryBuilder = new QueryQueryBuilder()
            .WithChains(new ChainQueryBuilder()
                .WithContractsOnChain(new ContractsOnChainQueryBuilder()
                    .WithRpc()
                    .WithContracts(new ComponentContractOnChainContractOnChainQueryBuilder()
                        .WithContractVersion(new ContractQueryBuilder()
                            .WithNameVersion()
                        )
                        .WithAddress(),
                        contractsFilter
                    )
                ),
                chainFilter,
                status: statusParam
            )
            .WithParameter(contractsFilter)
            .WithParameter(chainFilter)
            .WithParameter(statusParam);

        return new GraphQLRequest
        {
            Query = queryBuilder.Build()
        };
    }
}