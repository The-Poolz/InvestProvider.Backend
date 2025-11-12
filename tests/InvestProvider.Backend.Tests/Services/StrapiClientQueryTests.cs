using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Collections;
using Poolz.Finance.CSharp.Strapi;
using FluentValidation;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Strapi.Models;
using InvestProvider.Backend.Services.Web3.Contracts;
using Net.Web3.EthereumWallet;
using Xunit;

namespace InvestProvider.Backend.Tests.Services;

public class StrapiClientQueryTests
{
    private class StubMessageHandler : HttpMessageHandler
    {
        private readonly string _json;

        public StubMessageHandler(object response)
        {
            _json = Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var msg = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(_json, System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(msg);
        }
    }

    private static void SetClient(StrapiClient client, GraphQLHttpClient http)
    {
        var field = typeof(StrapiClient).GetField("_client", BindingFlags.NonPublic | BindingFlags.Instance)!;
        field.SetValue(client, http);
    }

    [Fact]
    public async Task ReceiveOnChainInfoAsync_ReturnsInfo()
    {
        Environment.SetEnvironmentVariable("STRAPI_GRAPHQL_URL", "http://localhost");

        var chain = new Chain
        {
            ChainId = 5,
            ContractsOnChain = new ContractsOnChain
            {
                Rpc = "http://rpc",
                Contracts = new List<ComponentContractOnChainContractOnChain>
                {
                    new ComponentContractOnChainContractOnChain
                    {
                        ContractVersion = new Contract { NameVersion = "InvestProvider v1" },
                        Address = "0x00000000000000000000000000000000000000aa"
                    },
                    new ComponentContractOnChainContractOnChain
                    {
                        ContractVersion = new Contract { NameVersion = "LockDealNFT v1" },
                        Address = "0x00000000000000000000000000000000000000bb"
                    }
                }
            }
        };

        var gqlResponse = new GraphQLResponse<OnChainInfoResponse> { Data = new OnChainInfoResponse(new[] { chain }) };
        var handler = new StubMessageHandler(gqlResponse);
        var stub = new GraphQLHttpClient(new GraphQLHttpClientOptions { EndPoint = new Uri("http://localhost") }, new NewtonsoftJsonSerializer(), new HttpClient(handler));
        var client = new StrapiClient();
        SetClient(client, stub);

        var chainId = 5;
        var result = await client.ReceiveOnChainInfoAsync(chainId);

        Assert.Equal("0x00000000000000000000000000000000000000aa", result.InvestedProvider.ToString());
        Assert.Equal("0x00000000000000000000000000000000000000bb", result.LockDealNFT.ToString());
    }

    [Fact]
    public async Task ReceiveOnChainInfoAsync_Throws_WhenChainMissing()
    {
        Environment.SetEnvironmentVariable("STRAPI_GRAPHQL_URL", "http://localhost");
        var gqlResponse = new GraphQLResponse<OnChainInfoResponse> { Data = new OnChainInfoResponse(Array.Empty<Chain>()) };
        var handler = new StubMessageHandler(gqlResponse);
        var stub = new GraphQLHttpClient(new GraphQLHttpClientOptions { EndPoint = new Uri("http://localhost") }, new NewtonsoftJsonSerializer(), new HttpClient(handler));
        var client = new StrapiClient();
        SetClient(client, stub);

        await Assert.ThrowsAsync<ValidationException>(() => client.ReceiveOnChainInfoAsync(7));
    }

    [Fact]
    public async Task ReceiveProjectInfoAsync_ReturnsInfo()
    {
        Environment.SetEnvironmentVariable("STRAPI_GRAPHQL_URL", "http://localhost");

        var phaseType = Type.GetType("Poolz.Finance.CSharp.Strapi.ComponentPhaseStartEndAmount, Poolz.Finance.CSharp.Strapi")!;
        var phase = Activator.CreateInstance(phaseType)!;
        phaseType.GetProperty("Id")?.SetValue(phase, "p1");

        var phasesList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(phaseType))!;
        phasesList.Add(phase);

        var chainType = Type.GetType("Poolz.Finance.CSharp.Strapi.Chain, Poolz.Finance.CSharp.Strapi")!;
        var chain = Activator.CreateInstance(chainType)!;
        chainType.GetProperty("ChainId")?.SetValue(chain, (long?)3);

        var chainSettingType = Type.GetType("Poolz.Finance.CSharp.Strapi.ChainSetting, Poolz.Finance.CSharp.Strapi")!;
        var chainSetting = Activator.CreateInstance(chainSettingType)!;
        chainSettingType.GetProperty("Chain")?.SetValue(chainSetting, chain);

        var projectsInfoType = Type.GetType("Poolz.Finance.CSharp.Strapi.ProjectsInformation, Poolz.Finance.CSharp.Strapi")!;
        var projectsInfo = Activator.CreateInstance(projectsInfoType)!;
        projectsInfoType.GetProperty("ChainSetting")?.SetValue(projectsInfo, chainSetting);
        projectsInfoType.GetProperty("ProjectPhases")?.SetValue(projectsInfo, phasesList);

        var gqlResponse = new GraphQLResponse<ProjectInfoResponse> { Data = new ProjectInfoResponse((dynamic)projectsInfo) };
        var handler = new StubMessageHandler(gqlResponse);
        var stub = new GraphQLHttpClient(new GraphQLHttpClientOptions { EndPoint = new Uri("http://localhost") }, new NewtonsoftJsonSerializer(), new HttpClient(handler));
        var client = new StrapiClient();
        SetClient(client, stub);

        var info = await client.ReceiveProjectInfoAsync("pid", false);
        Assert.Equal(3, info.ChainId);
        Assert.Single(info.Phases);
    }

    [Fact]
    public async Task ReceiveProjectInfoAsync_Throws_WhenProjectMissing()
    {
        Environment.SetEnvironmentVariable("STRAPI_GRAPHQL_URL", "http://localhost");
        var gqlResponse = new GraphQLResponse<ProjectInfoResponse> { Data = new ProjectInfoResponse(null!) };
        var handler = new StubMessageHandler(gqlResponse);
        var stub = new GraphQLHttpClient(new GraphQLHttpClientOptions { EndPoint = new Uri("http://localhost") }, new NewtonsoftJsonSerializer(), new HttpClient(handler));
        var client = new StrapiClient();
        SetClient(client, stub);

        await Assert.ThrowsAsync<ValidationException>(() => client.ReceiveProjectInfoAsync("pid", true));
    }

    [Fact]
    public void SendQuery_Throws_OnGraphQlErrors()
    {
        Environment.SetEnvironmentVariable("STRAPI_GRAPHQL_URL", "http://localhost");

        var errorResp = new GraphQLResponse<OnChainInfoResponse>
        {
            Errors = new[] { new GraphQLError { Message = "err1" }, new GraphQLError { Message = "err2" } },
            Data = new OnChainInfoResponse(Array.Empty<Chain>())
        };
        var handler = new StubMessageHandler(errorResp);
        var stub = new GraphQLHttpClient(new GraphQLHttpClientOptions { EndPoint = new Uri("http://localhost") }, new NewtonsoftJsonSerializer(), new HttpClient(handler));
        var client = new StrapiClient();
        SetClient(client, stub);

        var method = typeof(StrapiClient).GetMethod("SendQueryAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var generic = method.MakeGenericMethod(typeof(OnChainInfoResponse));

        void Act()
        {
            try
            {
                var task = (Task)generic.Invoke(client, new object[] { new GraphQLRequest() })!;
                task.GetAwaiter().GetResult();
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException!;
            }
        }

        var ex = Assert.Throws<InvalidOperationException>(Act);
        Assert.Contains("err1", ex.Message);
        Assert.Contains("err2", ex.Message);
    }
}
