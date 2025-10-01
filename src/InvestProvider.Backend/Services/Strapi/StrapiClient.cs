using GraphQL;
using GraphQL.Client.Http;
using System.Net.Http.Headers;
using Net.Web3.EthereumWallet;
using Poolz.Finance.CSharp.Strapi;
using EnvironmentManager.Extensions;
using Net.Utils.ErrorHandler.Extensions;
using GraphQL.Client.Serializer.Newtonsoft;
using InvestProvider.Backend.Services.Strapi.Models;
using InvestProvider.Backend.Services.Web3.Contracts;
using ProjectInfo = InvestProvider.Backend.Services.Strapi.Models.ProjectInfo;

namespace InvestProvider.Backend.Services.Strapi;

public class StrapiClient : IStrapiClient
{
    public static readonly string ApiUrl = Env.STRAPI_GRAPHQL_URL.GetRequired<string>();

    private readonly GraphQLHttpClient _client = new(
        new GraphQLHttpClientOptions { EndPoint = new Uri(ApiUrl) },
        new NewtonsoftJsonSerializer(),
        new HttpClient
        {
            DefaultRequestHeaders =
            {
                CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    NoStore = true,
                    MustRevalidate = true
                }
            }
        }
    );

    public async Task<OnChainInfo> ReceiveOnChainInfoAsync(long chainId)
    {
        var response = await SendQueryAsync<OnChainInfoResponse>(OnChainInfoRequest.BuildRequest(chainId));

        if (response.Data.Chains.Count == 0 || response.Data.Chains.First().ContractsOnChain == null)
        {
            throw Error.CHAIN_NOT_SUPPORTED.ToException(new
            {
                ChainId = chainId
            });
        }

        var chain = response.Data.Chains.First();
        var investedProvider = ExtractAddress(chain, ContractNames.InvestProvider, Error.INVESTED_PROVIDER_NOT_SUPPORTED);
        var lockDealNFT = ExtractAddress(chain, ContractNames.LockDealNFT, Error.LOCK_DEAL_NFT_NOT_SUPPORTED);

        return new OnChainInfo(investedProvider, lockDealNFT);
    }

    public async Task<ProjectInfo> ReceiveProjectInfoAsync(string projectId, bool filterPhases)
    {
        var response = await SendQueryAsync<ProjectInfoResponse>(ProjectPhaseRequest.BuildRequest(projectId, filterPhases));

        if (response.Data.ProjectsInfo == null)
        {
            throw Error.PROJECT_PHASE_NOT_FOUND.ToException(new
            {
                ProjectId = projectId
            });
        }

        return new ProjectInfo(response.Data);
    }

    private async Task<GraphQLResponse<TResponse>> SendQueryAsync<TResponse>(GraphQLRequest request)
    {
        var response = await _client.SendQueryAsync<TResponse>(request);

        if (response.Errors != null && response.Errors.Length != 0)
        {
            var errorMessage = string.Join(Environment.NewLine, response.Errors.Select(x => x.Message));
            throw new InvalidOperationException(errorMessage);
        }

        return response;
    }

    private static EthereumAddress ExtractAddress(Chain chain, string nameOfContract, Error notFoundError)
    {
        var contract = chain.ContractsOnChain.Contracts.FirstOrDefault(x =>
            x.ContractVersion.NameVersion.Contains(nameOfContract)
        );
        return contract == null
            ? throw notFoundError.ToException(new
        {
            chain.ChainId
        })
            : (EthereumAddress)contract.Address;
    }
}