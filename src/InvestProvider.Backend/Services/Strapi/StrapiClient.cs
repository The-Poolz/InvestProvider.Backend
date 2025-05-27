using GraphQL;
using GraphQL.Client.Http;
using Net.Web3.EthereumWallet;
using Poolz.Finance.CSharp.Strapi;
using EnvironmentManager.Extensions;
using Net.Utils.ErrorHandler.Extensions;
using GraphQL.Client.Serializer.Newtonsoft;
using InvestProvider.Backend.Services.Strapi.Models;
using ProjectInfo = InvestProvider.Backend.Services.Strapi.Models.ProjectInfo;

namespace InvestProvider.Backend.Services.Strapi;

public class StrapiClient : IStrapiClient
{
    public const string NameOfInvestedProvider = "InvestProvider";
    public const string NameOfLockDealNFT = "LockDealNFT";
    public static readonly string ApiUrl = Env.STRAPI_GRAPHQL_URL.GetRequired<string>();

    private readonly GraphQLHttpClient _client = new(
        new GraphQLHttpClientOptions { EndPoint = new Uri(ApiUrl) },
        new NewtonsoftJsonSerializer()
    );

    public OnChainInfo ReceiveOnChainInfo(long chainId)
    {
        var response = SendQuery<OnChainInfoResponse>(OnChainInfoRequest.BuildRequest(chainId), graphQlResponse =>
        {
            if (!graphQlResponse.Data.Chains.Any() || graphQlResponse.Data.Chains.First().ContractsOnChain == null)
            {
                throw Error.CHAIN_NOT_SUPPORTED.ToException(new
                {
                    ChainId = chainId
                });
            }
        });

        var chain = response.Data.Chains.First();
        var investedProvider = ExtractAddress(chain, NameOfInvestedProvider, Error.INVESTED_PROVIDER_NOT_SUPPORTED);
        var lockDealNFT = ExtractAddress(chain, NameOfLockDealNFT, Error.LOCK_DEAL_NFT_NOT_SUPPORTED);

        return new OnChainInfo(chain.ContractsOnChain.Rpc, investedProvider, lockDealNFT);
    }

    public ProjectInfo ReceiveProjectInfo(string phaseId)
    {
        var response = SendQuery<ProjectInfoResponse>(ProjectPhaseRequest.BuildRequest(phaseId), graphQlResponse =>
        {
            if (graphQlResponse.Data.ProjectsInfo == null)
            {
                throw Error.PROJECT_PHASE_NOT_FOUND.ToException(new
                {
                    PhaseId = phaseId
                });
            }
        });

        return new ProjectInfo(response.Data);
    }

    private GraphQLResponse<TResponse> SendQuery<TResponse>(GraphQLRequest request, Action<GraphQLResponse<TResponse>> notFoundHandlerFunc)
    {
        var response = _client.SendQueryAsync<TResponse>(request)
            .GetAwaiter()
            .GetResult();

        if (response.Errors != null && response.Errors.Any())
        {
            var errorMessage = string.Join(Environment.NewLine, response.Errors.Select(x => x.Message));
            throw new InvalidOperationException(errorMessage);
        }

        notFoundHandlerFunc.Invoke(response);

        return response;
    }

    private static EthereumAddress ExtractAddress(Chain chain, string nameOfContract, Error notFoundError)
    {
        var contract = chain.ContractsOnChain.Contracts.FirstOrDefault(x =>
            x.ContractVersion.NameVersion.Contains(nameOfContract)
        );
        if (contract == null) throw notFoundError.ToException(new
        {
            chain.ChainId
        });
        return contract.Address;
    }
}