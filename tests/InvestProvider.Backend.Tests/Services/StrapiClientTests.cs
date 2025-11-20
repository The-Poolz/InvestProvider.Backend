using Xunit;
using System;
using FluentValidation;
using System.Reflection;
using Net.Web3.EthereumWallet;
using System.Collections.Generic;
using Poolz.Finance.CSharp.Strapi;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Web3.Contracts;

namespace InvestProvider.Backend.Tests.Services;

public class StrapiClientTests
{
    [Fact]
    public void ExtractAddress_ReturnsAddress_ForExistingContract()
    {
        Environment.SetEnvironmentVariable("STRAPI_GRAPHQL_URL", "http://localhost");

        var chain = new Chain
        {
            ChainId = 1,
            ContractsOnChain = new ContractsOnChain
            {
                Rpc = "http://rpc",
                Contracts = new List<ComponentContractOnChainContractOnChain>
                {
                    new ComponentContractOnChainContractOnChain
                    {
                        ContractVersion = new Contract { NameVersion = "InvestProvider v1" },
                        Address = "0x00000000000000000000000000000000000000AA"
                    },
                    new ComponentContractOnChainContractOnChain
                    {
                        ContractVersion = new Contract { NameVersion = "LockDealNFT v1" },
                        Address = "0x00000000000000000000000000000000000000bb"
                    }
                }
            }
        };

        var method = typeof(StrapiClient).GetMethod("ExtractAddress", BindingFlags.NonPublic | BindingFlags.Static)!;
        var result = (EthereumAddress)method.Invoke(null, new object[] { chain, ContractNames.InvestProvider, Error.INVESTED_PROVIDER_NOT_SUPPORTED })!;

        Assert.Equal("0x00000000000000000000000000000000000000AA", result.ToString());
    }

    [Fact]
    public void ExtractAddress_Throws_WhenContractMissing()
    {
        Environment.SetEnvironmentVariable("STRAPI_GRAPHQL_URL", "http://localhost");

        var chain = new Chain
        {
            ChainId = 2,
            ContractsOnChain = new ContractsOnChain
            {
                Rpc = "http://rpc",
                Contracts = new List<ComponentContractOnChainContractOnChain>()
            }
        };

        var method = typeof(StrapiClient).GetMethod("ExtractAddress", BindingFlags.NonPublic | BindingFlags.Static)!;

        void Act()
        {
            try
            {
                method.Invoke(null, new object[] { chain, ContractNames.InvestProvider, Error.INVESTED_PROVIDER_NOT_SUPPORTED });
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException!;
            }
        }

        Assert.Throws<ValidationException>(Act);
    }
}
